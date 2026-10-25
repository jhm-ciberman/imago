using System;
using System.IO;
using System.Linq;
using System.Threading;
using Imago.Input;
using NeoVeldrid;

namespace Imago.ScenarioRunner;

/// <summary>
/// Runs a scenario's application on its own loop thread, inside a script process.
/// </summary>
internal sealed class ScenarioHost : IDisposable
{
    private const GraphicsBackend DefaultBackend = GraphicsBackend.OpenGL;
    private static readonly TimeSpan WatchdogTimeout = TimeSpan.FromMinutes(5.0);

    private readonly LoopSynchronizationContext _loop = new();
    private readonly TextWriter _console;
    private readonly TextWriter _log;

    private ScenarioHost(string outputDirectory)
    {
        this.OutputDirectory = outputDirectory;

        Directory.CreateDirectory(outputDirectory);

        // The script itself lives in this folder, so only the shots go.
        foreach (string file in Directory.EnumerateFiles(outputDirectory, "*.png"))
        {
            File.Delete(file);
        }

        // The game's own console output goes to the log file, so the console only shows the script's Log lines.
        this._console = Console.Out;
        this._log = new StreamWriter(Path.Combine(outputDirectory, "log.txt"), append: false) { AutoFlush = true };
        Console.SetOut(this._log);
    }

    /// <summary>
    /// Gets the folder the screenshots and the log are written to.
    /// </summary>
    public string OutputDirectory { get; }

    /// <summary>
    /// Builds the application on a new loop thread and drives the given scenario with it.
    /// </summary>
    /// <typeparam name="TScenario">The type of the scenario.</typeparam>
    /// <param name="scenario">The scenario to drive.</param>
    /// <param name="builder">The builder of the application, already configured by the scenario.</param>
    /// <param name="backend">The graphics backend, or null to read it from the command line.</param>
    /// <param name="show">Whether to open a real window, even if the command line does not ask for it.</param>
    /// <param name="outputDirectory">The folder the screenshots and the log are written to.</param>
    /// <returns>An awaitable that yields the scenario on the game loop thread once the application is running.</returns>
    public static ScenarioStart<TScenario> Start<TScenario>(
        TScenario scenario,
        ApplicationBuilder builder,
        GraphicsBackend? backend,
        bool show,
        string outputDirectory
    )
        where TScenario : Scenario
    {
        string[] args = Environment.GetCommandLineArgs();
        builder
            .WithBackend(backend ?? ReadBackend(args))
            .WithWindowState(WindowState.Normal)
            .WithHeadless(!show && !args.Contains("--show"));

        var host = new ScenarioHost(outputDirectory);
        StartWatchdog();

        InputManager.UseVirtualInput = true;

        var start = new ScenarioStart<TScenario>(scenario, host._loop);

        // The graphics context belongs to the thread that creates it, so the app is built on the loop thread too.
        var thread = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(host._loop);

            var app = builder.Build();

            // Advance the simulation by a fixed step each frame so animated content lands at the same phase every run.
            app.Ticker.FixedDeltaTime = 1.0 / 60.0;

            bool isStarted = false;
            app.Ticker.Ticked += (sender, e) =>
            {
                if (!isStarted)
                {
                    isStarted = true;
                    scenario.Attach(app, host);
                    start.MarkStarted();
                }

                host._loop.Drain();
                scenario.Tick(e.DeltaTime);
            };

            app.Run();
        })
        {
            IsBackground = true,
            Name = "game-loop",
        };

        thread.Start();
        return start;
    }

    /// <summary>
    /// Writes a line to the console and to the log file.
    /// </summary>
    /// <param name="message">The message to write.</param>
    public void Log(string message)
    {
        this._console.WriteLine($"  · {message}");
        this._log.WriteLine(message);
    }

    /// <summary>
    /// Gives the console back to the process and closes the log file.
    /// </summary>
    public void Dispose()
    {
        Console.SetOut(this._console);
        this._log.Dispose();
    }

    private static GraphicsBackend ReadBackend(string[] args)
    {
        int index = Array.IndexOf(args, "--backend");
        if (index < 0)
        {
            return DefaultBackend;
        }

        if (index + 1 < args.Length && Enum.TryParse(args[index + 1], ignoreCase: true, out GraphicsBackend backend))
        {
            return backend;
        }

        throw new ArgumentException($"--backend needs one of: {string.Join(", ", Enum.GetNames<GraphicsBackend>())}.");
    }

    private static void StartWatchdog()
    {
        var thread = new Thread(() =>
        {
            Thread.Sleep(WatchdogTimeout);
            Console.Error.WriteLine($"Watchdog fired after {WatchdogTimeout.TotalSeconds:0}s. Forcing exit.");
            Environment.Exit(124);
        })
        {
            IsBackground = true,
            Name = "scenario-watchdog",
        };

        thread.Start();
    }
}
