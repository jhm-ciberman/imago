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
/// Configures and runs the scenarios for a game. Create one with <see cref="ScenarioRunner.For{TGame}"/>, chain
/// the optional <c>With...</c> methods, and call <see cref="Run(string[])"/>.
/// </summary>
/// <typeparam name="TGame">The game application the scenarios drive.</typeparam>
public sealed class ScenarioRunnerBuilder<TGame> where TGame : Application
{
    private bool _autoCwd;
    private (int Width, int Height)? _windowSize;
    private GraphicsBackend? _backend;
    private Func<GraphicsBackend?, TGame>? _factory;
    private Action? _reset;

    internal ScenarioRunnerBuilder()
    {
    }

    /// <summary>
    /// Sets the working directory to the nearest ancestor of the executable that contains a solution file, so the
    /// game resolves its content paths the way it does in development. Does nothing if no solution file is found.
    /// </summary>
    /// <returns>This builder, for chaining.</returns>
    public ScenarioRunnerBuilder<TGame> WithAutoCwd()
    {
        this._autoCwd = true;
        return this;
    }

    /// <summary>
    /// Sets the window size for every scenario that does not declare its own <see cref="WindowSizeAttribute"/>.
    /// Without this, each scenario uses whatever size the application gives itself.
    /// </summary>
    /// <param name="width">The window width, in pixels.</param>
    /// <param name="height">The window height, in pixels.</param>
    /// <returns>This builder, for chaining.</returns>
    public ScenarioRunnerBuilder<TGame> WithWindowSize(int width, int height)
    {
        this._windowSize = (width, height);
        return this;
    }

    /// <summary>
    /// Sets the graphics backend for every scenario that does not declare its own <see cref="BackendAttribute"/>.
    /// The <c>--backend</c> command-line option overrides this. Without either, the application picks its own.
    /// </summary>
    /// <param name="backend">The graphics backend to use.</param>
    /// <returns>This builder, for chaining.</returns>
    public ScenarioRunnerBuilder<TGame> WithBackend(GraphicsBackend backend)
    {
        this._backend = backend;
        return this;
    }

    /// <summary>
    /// Sets a factory used to construct the application, instead of the default constructor lookup. Use this when
    /// the application has no public <c>(GraphicsBackend?)</c> or parameterless constructor.
    /// </summary>
    /// <param name="factory">A delegate that creates the application for a resolved backend.</param>
    /// <returns>This builder, for chaining.</returns>
    public ScenarioRunnerBuilder<TGame> WithFactory(Func<GraphicsBackend?, TGame> factory)
    {
        this._factory = factory;
        return this;
    }

    /// <summary>
    /// Sets the action that returns the game to a clean baseline between scenarios when several share a process,
    /// such as navigating to a neutral screen and dismissing popups. Runs on the game loop thread. Without it,
    /// each scenario simply inherits whatever the previous one left, relying on the game's own navigation to
    /// reset state.
    /// </summary>
    /// <param name="reset">A delegate that returns the game to its baseline.</param>
    /// <returns>This builder, for chaining.</returns>
    public ScenarioRunnerBuilder<TGame> WithReset(Action reset)
    {
        this._reset = reset;
        return this;
    }

    /// <summary>
    /// Runs the scenarios using the process command line.
    /// </summary>
    /// <returns>Zero when every scenario ran without error, or a non-zero exit code otherwise.</returns>
    public int Run()
    {
        return this.Run(Environment.GetCommandLineArgs()[1..]);
    }

    /// <summary>
    /// Runs the scenarios according to the given command line. Accepts any number of scenario names,
    /// <c>--filter &lt;text&gt;</c>, <c>--backend &lt;name&gt;</c>, <c>--isolated</c>, <c>--show</c>, and <c>--list</c>.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>Zero when every scenario ran without error, or a non-zero exit code otherwise.</returns>
    public int Run(string[] args)
    {
        if (this._autoCwd)
        {
            SetAutoCwd();
        }

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

        var command = ParseArgs(args);

        if (command.ShowList)
        {
            PrintList(scenarios);
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

        IReadOnlyList<ScenarioDescriptor> selected;
        if (command.Names.Count > 0)
        {
            var resolved = new List<ScenarioDescriptor>(command.Names.Count);
            foreach (var name in command.Names)
            {
                var scenario = scenarios.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
                if (scenario == null)
                {
                    Console.Error.WriteLine($"Unknown scenario: '{name}'.");
                    Console.Error.WriteLine();
                    PrintList(scenarios);
                    return 2;
                }

                resolved.Add(scenario);
            }

            selected = resolved;
        }
        else if (command.Filter != null)
        {
            selected = scenarios.Where(s => s.Name.Contains(command.Filter, StringComparison.OrdinalIgnoreCase)).ToList();
            if (selected.Count == 0)
            {
                Console.Error.WriteLine($"No scenarios match filter '{command.Filter}'.");
                return 2;
            }
        }
        else
        {
            selected = scenarios;
        }

        // A worker runs the batch it was handed in one process. A lone scenario does too, since isolating it
        // from a one-item batch would change nothing. Everything else is the orchestrator: it groups the
        // scenarios into batches and launches a worker process for each.
        if (command.Worker || selected.Count == 1)
        {
            return this.RunInProcess(selected, cliBackend, command.ShowWindow);
        }

        return this.Orchestrate(selected, command.BackendName, cliBackend, command.ShowWindow, command.Isolated);
    }

    private int RunInProcess(IReadOnlyList<ScenarioDescriptor> scenarios, GraphicsBackend? cliBackend, bool showWindow)
    {
        StartWatchdog(TimeSpan.FromSeconds(120.0 * scenarios.Count));

        // Scenario runs use a virtual mouse and keyboard, so a person can move the real mouse and type freely
        // while a run is in progress without affecting it, and the real cursor is never moved or captured.
        InputManager.UseVirtualInput = true;

        // Capture is off-screen, so by default the window is created hidden and never appears. Pass --show to
        // watch the scenarios run in a real window.
        Application.Headless = !showWindow;

        // The backend is fixed for the life of the process, so a batch shares one; the orchestrator only ever
        // groups same-backend scenarios together. Window size, by contrast, can change between scenarios.
        GraphicsBackend? backend = scenarios[0].Backend ?? cliBackend ?? this._backend;
        TGame app = this.CreateApp(backend);

        app.Window.WindowState = showWindow ? WindowState.Normal : WindowState.Hidden;
        if (this._windowSize is { } size)
        {
            app.Window.Width = size.Width;
            app.Window.Height = size.Height;
        }

        using var driver = new ScenarioDriver(app, scenarios, this._reset, this._windowSize);
        app.Ticker.Ticked += (sender, e) => driver.Tick((float)e.DeltaTime);
        app.Run();

        return driver.ExitCode;
    }

    private TGame CreateApp(GraphicsBackend? backend)
    {
        if (this._factory != null)
        {
            return this._factory(backend);
        }

        var backendCtor = typeof(TGame).GetConstructor([typeof(GraphicsBackend?)]);
        if (backendCtor != null)
        {
            return (TGame)backendCtor.Invoke([backend]);
        }

        var defaultCtor = typeof(TGame).GetConstructor(Type.EmptyTypes);
        if (defaultCtor != null)
        {
            return (TGame)defaultCtor.Invoke(null);
        }

        throw new InvalidOperationException(
            $"{typeof(TGame).Name} needs a public constructor taking a single GraphicsBackend? or no parameters, or a factory via WithFactory.");
    }

    private int Orchestrate(
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

        var batches = this.GroupIntoBatches(scenarios, cliBackend, forceIsolated);

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

    private List<List<ScenarioDescriptor>> GroupIntoBatches(
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
            // backend means "let the app pick", which resolves to the same choice for all of them, so they group.
            GraphicsBackend? backend = scenario.Backend ?? cliBackend ?? this._backend;
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

    private static CommandLine ParseArgs(string[] args)
    {
        var names = new List<string>();
        string? filter = null;
        string? backendName = null;
        bool showList = false;
        bool showWindow = false;
        bool isolated = false;
        bool worker = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--list":
                    showList = true;
                    break;
                case "--show":
                    showWindow = true;
                    break;
                case "--isolated":
                    isolated = true;
                    break;
                case "--worker":
                    worker = true;
                    break;
                case "--filter" when i + 1 < args.Length:
                    filter = args[++i];
                    break;
                case "--backend" when i + 1 < args.Length:
                    backendName = args[++i];
                    break;
                default:
                    if (!args[i].StartsWith('-'))
                    {
                        names.Add(args[i]);
                    }

                    break;
            }
        }

        return new CommandLine(names, filter, backendName, showList, showWindow, isolated, worker);
    }

    private static void PrintList(IReadOnlyList<ScenarioDescriptor> scenarios)
    {
        string executable = Path.GetFileNameWithoutExtension(Environment.ProcessPath) ?? "scenarios";
        Console.WriteLine($"Usage: {executable} [scenario...] [--filter <text>] [--backend <name>] [--isolated] [--show] [--list]");
        Console.WriteLine("  With no scenario name, every scenario runs. Scenarios sharing a backend share a process;");
        Console.WriteLine("  pass --isolated to run each in its own process instead.");
        Console.WriteLine();
        Console.WriteLine("Scenarios:");
        foreach (var scenario in scenarios)
        {
            string isolated = scenario.Isolated ? "  [isolated]" : string.Empty;
            Console.WriteLine($"  {scenario.Name,-18} {scenario.Description}{isolated}");
        }
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

    private sealed record CommandLine(
        IReadOnlyList<string> Names,
        string? Filter,
        string? BackendName,
        bool ShowList,
        bool ShowWindow,
        bool Isolated,
        bool Worker);
}
