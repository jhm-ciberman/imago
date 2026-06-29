using System;
using System.Collections.Generic;
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

        if (command.ShowList)
        {
            ScenarioCommandLine.PrintList(scenarios);
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

        // A worker runs the batch it was handed in one process. A lone scenario does too, since isolating it from
        // a one-item batch would change nothing. Everything else is orchestrated across worker processes.
        if (command.Worker || selected.Count == 1)
        {
            return this.RunInProcess(selected, cliBackend, command.ShowWindow);
        }

        return ScenarioOrchestrator.Run(selected, command.BackendName, cliBackend, command.ShowWindow, command.Isolated);
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

        GraphicsBackend? backend = scenarios[0].Backend ?? cliBackend;
        Application app = this.CreateApp(backend);
        app.Window.WindowState = showWindow ? WindowState.Normal : WindowState.Hidden;

        using var driver = new ScenarioDriver(app, scenarios, () => this.Reset(app));
        app.Ticker.Ticked += (sender, e) => driver.Tick((float)e.DeltaTime);
        app.Run();

        return driver.ExitCode;
    }

    private static IReadOnlyList<ScenarioDescriptor>? Select(IReadOnlyList<ScenarioDescriptor> scenarios, ScenarioCommandLine command)
    {
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
                    ScenarioCommandLine.PrintList(scenarios);
                    return null;
                }

                resolved.Add(scenario);
            }

            return resolved;
        }

        if (command.Filter != null)
        {
            var matches = scenarios.Where(s => s.Name.Contains(command.Filter, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count == 0)
            {
                Console.Error.WriteLine($"No scenarios match filter '{command.Filter}'.");
                return null;
            }

            return matches;
        }

        return scenarios;
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
