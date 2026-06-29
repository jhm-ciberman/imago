using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Imago.Input;
using NeoVeldrid;

namespace Imago.ScenarioRunner;

/// <summary>
/// Base class for a game's scenario runner. A game derives from this, overrides <see cref="CreateApp"/> to build
/// its application and <see cref="Reset"/> to return it to a baseline between scenarios, and calls
/// <see cref="Run(string[])"/> from its entry point.
/// </summary>
/// <remarks>
/// Booting the game and loading its content is the slow part of a run, so scenarios that share a graphics backend
/// run together in one process: it boots once and plays each in turn, calling <see cref="Reset"/> between them.
/// A scenario marked <see cref="IsolatedAttribute"/>, or any run passed <c>--isolated</c>, gets its own process,
/// and so does a scenario whose <see cref="BackendAttribute"/> differs.
/// </remarks>
public abstract class ImagoScenarioRunner
{
    /// <summary>
    /// Builds the application the scenarios drive. Set its window size and any other construction-time options here.
    /// </summary>
    /// <param name="backend">The graphics backend resolved from <c>--backend</c> or a scenario's attribute, or null to let the application pick.</param>
    /// <returns>The application to run the scenarios on.</returns>
    protected abstract Application CreateApp(GraphicsBackend? backend);

    /// <summary>
    /// Returns the game to the baseline a scenario expects when several share a process, such as navigating to a
    /// neutral screen and dismissing popups. Runs on the game loop thread between scenarios. The default does nothing.
    /// </summary>
    /// <param name="app">The application being driven.</param>
    protected virtual void Reset(Application app)
    {
    }

    /// <summary>
    /// Runs the scenarios according to the given command line. Accepts any number of scenario names,
    /// <c>--filter &lt;text&gt;</c>, <c>--backend &lt;name&gt;</c>, <c>--isolated</c>, <c>--show</c>, and <c>--list</c>.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>Zero when every scenario ran without error, or a non-zero exit code otherwise.</returns>
    public int Run(string[] args)
    {
        SetAutoCwd();

        IReadOnlyList<ScenarioDescriptor> scenarios;
        try
        {
            scenarios = ScenarioRegistry.All(Assembly.GetEntryAssembly()!);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Could not load scenarios: {ex.Message}");
            return 2;
        }

        var command = ScenarioCommandLine.Parse(args);

        if (command.ShowHelp)
        {
            ScenarioCommandLine.PrintHelp(scenarios);
            return 0;
        }

        if (command.UnknownOptions.Count > 0)
        {
            foreach (var option in command.UnknownOptions)
            {
                Console.Error.WriteLine($"Unknown option '{option}'.");
            }

            Console.Error.WriteLine("Run with --help to see the available options.");
            return 2;
        }

        if (command.ShowList)
        {
            ScenarioCommandLine.PrintScenarios(scenarios);
            return 0;
        }

        GraphicsBackend? cliBackend = null;
        if (command.BackendName != null)
        {
            if (!Enum.TryParse(command.BackendName, ignoreCase: true, out GraphicsBackend parsed))
            {
                Console.Error.WriteLine($"Unknown backend '{command.BackendName}'. Valid backends: {string.Join(", ", Enum.GetNames<GraphicsBackend>())}.");
                return 2;
            }

            cliBackend = parsed;
        }

        var selected = Select(scenarios, command);
        if (selected == null)
        {
            return 2;
        }

        // Capture the real output before any redirect, so the presenter keeps writing here while the game's
        // own console output is sent elsewhere.
        var presenter = new ConsolePresenter(Console.Out);

        // A worker runs in this process the batch it was handed, writing its results to the report file.
        if (command.Worker)
        {
            return this.RunInProcess(selected, cliBackend, command.ShowWindow, command.ReportPath, presenter);
        }

        // A single batch (the common case) runs in this process, where all the data for the summary already
        // lives. Only several batches, isolation or differing backends, are spread across worker processes.
        var batches = ScenarioBatcher.Group(selected, cliBackend, command.Isolated);
        int exitCode = batches.Count == 1
            ? this.RunInProcess(batches[0], cliBackend, command.ShowWindow, reportPath: null, presenter)
            : ScenarioOrchestrator.Run(batches, command.BackendName, command.ShowWindow, presenter);

        HandleBaseline(command, presenter);
        return exitCode;
    }

    private static void HandleBaseline(ScenarioCommandLine command, ConsolePresenter presenter)
    {
        if (command.Baseline)
        {
            SaveBaseline(ScenarioPaths.Shots, ScenarioPaths.Baseline);
            presenter.BaselineSaved(ScenarioPaths.Baseline);
            return;
        }

        if (Directory.Exists(ScenarioPaths.Baseline))
        {
            var changes = ScenarioComparer.Compare(ScenarioPaths.Baseline, ScenarioPaths.Shots, ScenarioPaths.Diff);
            presenter.Changes(changes, ScenarioPaths.Diff);
        }
    }

    private static void SaveBaseline(string currentDir, string baselineDir)
    {
        if (Directory.Exists(baselineDir))
        {
            Directory.Delete(baselineDir, recursive: true);
        }

        foreach (string file in Directory.EnumerateFiles(currentDir, "*.png", SearchOption.AllDirectories))
        {
            string destination = Path.Combine(baselineDir, Path.GetRelativePath(currentDir, file));
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: true);
        }
    }

    private int RunInProcess(
        IReadOnlyList<ScenarioDescriptor> scenarios,
        GraphicsBackend? cliBackend,
        bool showWindow,
        string? reportPath,
        ConsolePresenter presenter)
    {
        var stopwatch = Stopwatch.StartNew();
        StartWatchdog(TimeSpan.FromSeconds(120.0 * scenarios.Count));

        // Scenario runs use a virtual mouse and keyboard, so a person can move the real mouse and type freely
        // while a run is in progress without affecting it, and the real cursor is never moved or captured.
        InputManager.UseVirtualInput = true;

        // Capture is off-screen, so by default the window is created hidden and never appears. Pass --show to
        // watch the scenarios run in a real window.
        Application.Headless = !showWindow;

        // A worker streams its lines but leaves the header and summary to the orchestrating parent.
        bool standalone = reportPath == null;
        if (standalone)
        {
            presenter.RunStarted(scenarios.Count, 1);
        }

        RedirectGameConsole();

        GraphicsBackend? backend = scenarios[0].Backend ?? cliBackend;
        Application app = this.CreateApp(backend);
        app.Window.WindowState = showWindow ? WindowState.Normal : WindowState.Hidden;
        string backendName = app.Renderer.BackendType.ToString().ToLowerInvariant();

        // Advance the simulation by a fixed step each frame so animated content lands at the same phase every
        // run; without this, captures drift with frame timing and the baseline comparison sees false changes.
        app.Ticker.FixedDeltaTime = 1.0 / 60.0;

        using var driver = new ScenarioDriver(app, scenarios, presenter, () => this.Reset(app));
        app.Ticker.Ticked += (sender, e) => driver.Tick((float)e.DeltaTime);
        app.Run();
        stopwatch.Stop();

        if (standalone)
        {
            presenter.RunFinished(driver.Passed, driver.Failed, driver.Shots, stopwatch.Elapsed.TotalSeconds, backendName, driver.WindowSize, 1);
        }
        else
        {
            new ScenarioReport(driver.Passed, driver.Failed, driver.Shots, backendName, driver.WindowSize).Write(reportPath!);
        }

        return driver.Failed > 0 ? 1 : 0;
    }

    private static IReadOnlyList<ScenarioDescriptor>? Select(IReadOnlyList<ScenarioDescriptor> scenarios, ScenarioCommandLine command)
    {
        if (command.Names.Count == 0)
        {
            return scenarios;
        }

        var resolved = new List<ScenarioDescriptor>(command.Names.Count);
        foreach (var name in command.Names)
        {
            var scenario = scenarios.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
            if (scenario == null)
            {
                Console.Error.WriteLine($"Unknown scenario: '{name}'.");
                Console.Error.WriteLine();
                ScenarioCommandLine.PrintScenarios(scenarios);
                return null;
            }

            resolved.Add(scenario);
        }

        return resolved;
    }

    private static void RedirectGameConsole()
    {
        // The game logs to the console as it boots and loads content. Send that to a file so the run's output
        // stays clean; the presenter keeps writing to the real output it captured earlier.
        Directory.CreateDirectory(ScenarioPaths.Root);
        Console.SetOut(new StreamWriter(ScenarioPaths.Log, append: false) { AutoFlush = true });
    }

    private static void SetAutoCwd()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (directory.GetFiles().Any(file => file.Extension is ".sln" or ".slnx"))
            {
                Environment.CurrentDirectory = directory.FullName;
                return;
            }

            directory = directory.Parent;
        }
    }

    private static void StartWatchdog(TimeSpan timeout)
    {
        var thread = new Thread(() =>
        {
            Thread.Sleep(timeout);
            Console.Error.WriteLine($"Scenario watchdog fired after {timeout.TotalSeconds:0}s. Forcing exit.");
            Environment.Exit(124);
        })
        {
            IsBackground = true,
            Name = "scenario-watchdog",
        };

        thread.Start();
    }
}
