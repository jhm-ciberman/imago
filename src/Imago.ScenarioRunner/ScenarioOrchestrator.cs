using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NeoVeldrid;

namespace Imago.ScenarioRunner;

/// <summary>
/// Runs a set of scenarios across child processes: it groups them into batches that can share one boot, then
/// launches a worker process for each and reports the results. The parent never boots the game itself, so a
/// worker that crashes only loses its own batch.
/// </summary>
internal static class ScenarioOrchestrator
{
    /// <summary>
    /// Groups the scenarios into batches and runs each in its own worker process.
    /// </summary>
    /// <param name="scenarios">The scenarios to run.</param>
    /// <param name="backendName">The backend name to forward to the workers, or null.</param>
    /// <param name="cliBackend">The parsed backend override used for grouping, or null.</param>
    /// <param name="showWindow">Whether the workers should show a real window.</param>
    /// <param name="forceIsolated">Whether every scenario should run in its own process.</param>
    /// <returns>Zero when every process ran without error, or a non-zero exit code otherwise.</returns>
    public static int Run(
        IReadOnlyList<ScenarioDescriptor> scenarios,
        string? backendName,
        GraphicsBackend? cliBackend,
        bool showWindow,
        bool forceIsolated)
    {
        string? executable = Environment.ProcessPath;
        if (executable == null)
        {
            Console.Error.WriteLine("Cannot locate the runner executable to launch scenarios in child processes.");
            return 2;
        }

        var batches = GroupIntoBatches(scenarios, cliBackend, forceIsolated);

        Console.WriteLine($"Running {scenarios.Count} scenario(s) in {batches.Count} process(es).");
        Console.WriteLine();

        int erroredProcesses = 0;
        foreach (var batch in batches)
        {
            Console.WriteLine($"-- {batch.Count} scenario(s): {string.Join(", ", batch.Select(s => s.Name))}");

            int exitCode = RunWorker(executable, batch, backendName, showWindow);
            if (exitCode != 0)
            {
                erroredProcesses++;
                Console.Error.WriteLine($"  Process exited with code {exitCode}.");
            }

            Console.WriteLine();
        }

        Console.WriteLine($"{scenarios.Count} scenario(s) across {batches.Count} process(es), {erroredProcesses} errored. Review the screenshots under artifacts/scenarios/.");
        return erroredProcesses == 0 ? 0 : 1;
    }

    private static List<List<ScenarioDescriptor>> GroupIntoBatches(
        IReadOnlyList<ScenarioDescriptor> scenarios,
        GraphicsBackend? cliBackend,
        bool forceIsolated)
    {
        var batches = new List<List<ScenarioDescriptor>>();
        var byBackend = new Dictionary<string, List<ScenarioDescriptor>>();

        foreach (var scenario in scenarios)
        {
            if (forceIsolated || scenario.Isolated)
            {
                batches.Add([scenario]);
                continue;
            }

            // Scenarios that share a backend can share a process; a differing backend can't, since the renderer
            // is built once per process. Window size differences are fine and handled live by the driver. A null
            // backend means "let the host pick", which resolves the same for all of them, so they group.
            GraphicsBackend? backend = scenario.Backend ?? cliBackend;
            string key = backend?.ToString() ?? string.Empty;
            if (!byBackend.TryGetValue(key, out var batch))
            {
                batch = [];
                byBackend[key] = batch;
                batches.Add(batch);
            }

            batch.Add(scenario);
        }

        return batches;
    }

    private static int RunWorker(string executable, IReadOnlyList<ScenarioDescriptor> batch, string? backendName, bool showWindow)
    {
        // No stream redirection: the worker writes its per-scenario progress straight to this console, live.
        var startInfo = new ProcessStartInfo(executable) { UseShellExecute = false };

        startInfo.ArgumentList.Add("--worker");
        foreach (var scenario in batch)
        {
            startInfo.ArgumentList.Add(scenario.Name);
        }

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
