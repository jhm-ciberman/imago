using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Imago.Support.Numerics;

namespace Imago.ScenarioRunner;

/// <summary>
/// Runs scenario batches across child processes and folds their results into one summary. The parent never
/// boots the game itself, so a worker that crashes only loses its own batch.
/// </summary>
internal static class ScenarioOrchestrator
{
    /// <summary>
    /// Launches a worker process for each batch and renders a unified summary once they all finish.
    /// </summary>
    /// <param name="batches">The batches to run, each in its own process.</param>
    /// <param name="backendName">The backend name to forward to the workers, or null.</param>
    /// <param name="showWindow">Whether the workers should show a real window.</param>
    /// <param name="presenter">The presenter that renders the run header and summary.</param>
    /// <returns>Zero when every scenario passed, or a non-zero exit code otherwise.</returns>
    public static int Run(
        IReadOnlyList<List<ScenarioDescriptor>> batches,
        string? backendName,
        bool showWindow,
        ConsolePresenter presenter)
    {
        string? executable = Environment.ProcessPath;
        if (executable == null)
        {
            presenter.Error("Cannot locate the runner executable to launch scenarios in child processes.");
            return 2;
        }

        int scenarioCount = batches.Sum(batch => batch.Count);
        presenter.RunStarted(scenarioCount, batches.Count);

        var stopwatch = Stopwatch.StartNew();
        int passed = 0;
        int failed = 0;
        int shots = 0;
        string backend = string.Empty;
        Vector2Int windowSize = default;

        foreach (var batch in batches)
        {
            string reportPath = Path.GetTempFileName();
            try
            {
                int exitCode = RunWorker(executable, batch, backendName, showWindow, reportPath);
                if (ScenarioReport.Read(reportPath) is { } report)
                {
                    passed += report.Passed;
                    failed += report.Failed;
                    shots += report.Shots;
                    backend = report.Backend;
                    windowSize = report.WindowSize;
                }
                else
                {
                    // No report means the worker died before writing one (a native crash); its scenarios are lost.
                    failed += batch.Count;
                    presenter.Error($"Process for {string.Join(", ", batch.Select(s => s.Name))} exited with code {exitCode} before reporting.");
                }
            }
            finally
            {
                File.Delete(reportPath);
            }
        }

        stopwatch.Stop();
        presenter.RunFinished(passed, failed, shots, stopwatch.Elapsed.TotalSeconds, backend, windowSize, batches.Count);
        return failed == 0 ? 0 : 1;
    }

    private static int RunWorker(string executable, IReadOnlyList<ScenarioDescriptor> batch, string? backendName, bool showWindow, string reportPath)
    {
        // No stream redirection: the worker writes its per-scenario progress straight to this console, live, and
        // its totals to the report file.
        var startInfo = new ProcessStartInfo(executable) { UseShellExecute = false };

        startInfo.ArgumentList.Add("--worker");
        foreach (var scenario in batch)
        {
            startInfo.ArgumentList.Add(scenario.Name);
        }

        startInfo.ArgumentList.Add("--report");
        startInfo.ArgumentList.Add(reportPath);

        if (backendName != null)
        {
            startInfo.ArgumentList.Add("--backend");
            startInfo.ArgumentList.Add(backendName);
        }

        if (showWindow)
        {
            startInfo.ArgumentList.Add("--show");
        }

        using var process = Process.Start(startInfo)!;
        if (!process.WaitForExit((120_000 * batch.Count) + 60_000))
        {
            process.Kill(entireProcessTree: true);
            return 124;
        }

        return process.ExitCode;
    }
}
