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
    /// Runs the scenarios using the process command line.
    /// </summary>
    /// <returns>Zero when every scenario ran without error, or a non-zero exit code otherwise.</returns>
    public int Run()
    {
        return this.Run(Environment.GetCommandLineArgs()[1..]);
    }

    /// <summary>
    /// Runs the scenarios according to the given command line. Accepts an optional scenario name,
    /// <c>--filter &lt;text&gt;</c>, <c>--backend &lt;name&gt;</c>, and <c>--list</c>.
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

        var (name, filter, backendName, showList) = ParseArgs(args);

        if (showList)
        {
            PrintList(scenarios);
            return 0;
        }

        GraphicsBackend? cliBackend = null;
        if (backendName != null)
        {
            if (!Enum.TryParse(backendName, ignoreCase: true, out GraphicsBackend parsed))
            {
                Console.Error.WriteLine($"Unknown backend '{backendName}'. Valid backends: {string.Join(", ", Enum.GetNames<GraphicsBackend>())}.");
                return 2;
            }

            cliBackend = parsed;
        }

        if (name != null)
        {
            var scenario = scenarios.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
            if (scenario == null)
            {
                Console.Error.WriteLine($"Unknown scenario: '{name}'.");
                Console.Error.WriteLine();
                PrintList(scenarios);
                return 2;
            }

            return this.RunSingle(scenario, cliBackend);
        }

        var selected = filter == null
            ? scenarios
            : scenarios.Where(s => s.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();

        if (selected.Count == 0)
        {
            Console.Error.WriteLine($"No scenarios match filter '{filter}'.");
            return 2;
        }

        return RunAll(selected, backendName);
    }

    private int RunSingle(ScenarioDescriptor scenario, GraphicsBackend? cliBackend)
    {
        string outputDirectory = Path.Combine("artifacts", "scenarios", scenario.Name);
        Directory.CreateDirectory(outputDirectory);

        StartWatchdog(TimeSpan.FromSeconds(120));

        // Scenario runs use a virtual mouse and keyboard, so a person can move the real mouse and type freely
        // while a run is in progress without affecting it, and the real cursor is never moved or captured.
        InputManager.UseVirtualInput = true;

        Console.WriteLine($"Running scenario '{scenario.Name}': {scenario.Description}");

        GraphicsBackend? backend = scenario.Backend ?? cliBackend ?? this._backend;
        TGame app = this.CreateApp(backend);

        (int Width, int Height)? windowSize = scenario.WindowSize ?? this._windowSize;
        if (windowSize is { } size)
        {
            app.Window.WindowState = WindowState.Normal;
            app.Window.Width = size.Width;
            app.Window.Height = size.Height;
        }

        using var driver = new ScenarioDriver(app, scenario, outputDirectory);
        app.Ticker.Ticked += (sender, e) => driver.Tick((float)e.DeltaTime);
        app.Run();

        if (driver.ExitCode == 0)
        {
            Console.WriteLine($"Executed. Review the screenshots in {Path.GetFullPath(outputDirectory)}");
        }

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

    private static int RunAll(IReadOnlyList<ScenarioDescriptor> scenarios, string? backendName)
    {
        string? executable = Environment.ProcessPath;
        if (executable == null)
        {
            Console.Error.WriteLine("Cannot locate the runner executable to launch scenarios in child processes.");
            return 2;
        }

        Console.WriteLine($"Running {scenarios.Count} scenario(s), each in its own process.");
        Console.WriteLine();

        int errored = 0;
        foreach (var scenario in scenarios)
        {
            Console.Out.Write($"  {scenario.Name,-20} ");
            Console.Out.Flush();

            var (exitCode, output) = RunChild(executable, scenario.Name, backendName);
            if (exitCode == 0)
            {
                Console.WriteLine($"ran    -> artifacts/scenarios/{scenario.Name}/");
            }
            else
            {
                errored++;
                Console.WriteLine($"ERROR  (exit {exitCode})");
                Console.Error.WriteLine(Indent(Tail(output, 12)));
            }
        }

        Console.WriteLine();
        Console.WriteLine($"{scenarios.Count} scenario(s) ran, {errored} errored. Review the screenshots under artifacts/scenarios/.");
        return errored == 0 ? 0 : 1;
    }

    private static (int ExitCode, string Output) RunChild(string executable, string scenarioName, string? backendName)
    {
        var startInfo = new ProcessStartInfo(executable)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        startInfo.ArgumentList.Add(scenarioName);
        if (backendName != null)
        {
            startInfo.ArgumentList.Add("--backend");
            startInfo.ArgumentList.Add(backendName);
        }

        using var process = Process.Start(startInfo)!;
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(180_000))
        {
            process.Kill(entireProcessTree: true);
            return (124, "Timed out.");
        }

        string output = outputTask.GetAwaiter().GetResult() + errorTask.GetAwaiter().GetResult();
        return (process.ExitCode, output);
    }

    private static (string? Name, string? Filter, string? BackendName, bool ShowList) ParseArgs(string[] args)
    {
        string? name = null;
        string? filter = null;
        string? backendName = null;
        bool showList = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--list":
                    showList = true;
                    break;
                case "--filter" when i + 1 < args.Length:
                    filter = args[++i];
                    break;
                case "--backend" when i + 1 < args.Length:
                    backendName = args[++i];
                    break;
                default:
                    if (!args[i].StartsWith('-') && name == null)
                    {
                        name = args[i];
                    }

                    break;
            }
        }

        return (name, filter, backendName, showList);
    }

    private static void PrintList(IReadOnlyList<ScenarioDescriptor> scenarios)
    {
        string executable = Path.GetFileNameWithoutExtension(Environment.ProcessPath) ?? "scenarios";
        Console.WriteLine($"Usage: {executable} [scenario] [--filter <text>] [--backend <name>] [--list]");
        Console.WriteLine("  With no scenario name, every scenario runs, each in its own process.");
        Console.WriteLine();
        Console.WriteLine("Scenarios:");
        foreach (var scenario in scenarios)
        {
            Console.WriteLine($"  {scenario.Name,-18} {scenario.Description}");
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

    private static string Tail(string text, int lines)
    {
        var split = text.TrimEnd().Replace("\r\n", "\n").Split('\n');
        return string.Join('\n', split.Skip(Math.Max(0, split.Length - lines)));
    }

    private static string Indent(string text)
    {
        return string.Join('\n', text.Split('\n').Select(line => "      " + line));
    }
}
