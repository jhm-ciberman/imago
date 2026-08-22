using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Imago.Input;
using Imago.Rendering;
using Imago.SceneGraph;
using Imago.Support;
using NeoVeldrid;
using SixLabors.ImageSharp;

namespace Imago.ScenarioRunner;

/// <summary>
/// Runs an <see cref="Application"/> from a script and saves the screenshots it captures. A game derives from
/// this to add helpers for its own screens and state.
/// </summary>
/// <remarks>
/// Every await in the script resumes on the game loop thread, so the script can touch the game freely.
/// </remarks>
public abstract class ScenarioBase : IDisposable
{
    private const double DefaultTimeoutSeconds = 20.0;
    private static readonly TimeSpan WatchdogTimeout = TimeSpan.FromMinutes(5.0);

    private readonly ScenarioScheduler _scheduler = new();
    private readonly LoopSynchronizationContext _loop = new();
    private readonly List<string> _shots = new();

    private Application? _app;
    private StageCapturer? _capturer;
    private TextWriter? _log;
    private TextWriter? _console;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScenarioBase"/> class.
    /// </summary>
    /// <param name="outputDirectory">The folder the screenshots and the log are written to.</param>
    protected ScenarioBase(string outputDirectory)
    {
        this.OutputDirectory = outputDirectory;
    }

    /// <summary>
    /// Gets the folder the screenshots and the log are written to.
    /// </summary>
    public string OutputDirectory { get; }

    /// <summary>
    /// Gets the application being driven.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no run is in progress.</exception>
    public Application App => this._app ?? throw new InvalidOperationException("No run is in progress.");

    /// <summary>
    /// Waits until the next frame has been rendered.
    /// </summary>
    /// <returns>A task that completes on the next frame.</returns>
    public Task NextFrame()
    {
        return this._scheduler.WaitFrames(1, DefaultTimeoutSeconds);
    }

    /// <summary>
    /// Waits the given number of frames, letting pending changes render before continuing.
    /// </summary>
    /// <param name="frames">The number of frames to wait.</param>
    /// <returns>A task that completes once the frames have elapsed.</returns>
    public Task Wait(int frames)
    {
        return this._scheduler.WaitFrames(frames, DefaultTimeoutSeconds);
    }

    /// <summary>
    /// Waits the given number of seconds of advanced time, letting pending changes render before continuing.
    /// </summary>
    /// <param name="seconds">The number of seconds to wait.</param>
    /// <returns>A task that completes once the time has elapsed.</returns>
    public Task Wait(double seconds)
    {
        return this._scheduler.WaitSeconds(seconds, seconds + DefaultTimeoutSeconds);
    }

    /// <summary>
    /// Waits until <paramref name="condition"/> holds, failing if it does not within the timeout.
    /// </summary>
    /// <param name="condition">The condition to poll once per frame.</param>
    /// <param name="because">A description of what is awaited, shown if the wait times out.</param>
    /// <param name="timeoutSeconds">The maximum time to wait, in seconds.</param>
    /// <returns>A task that completes when the condition holds.</returns>
    public Task WaitUntil(Func<bool> condition, string because, double timeoutSeconds = DefaultTimeoutSeconds)
    {
        return this._scheduler.WaitUntil(condition, because, timeoutSeconds);
    }

    /// <summary>
    /// Moves the virtual cursor to a screen position, in window pixels, so hover and picking follow it.
    /// </summary>
    /// <param name="screenPosition">The cursor position in window pixels.</param>
    public void MoveCursor(Vector2 screenPosition)
    {
        InputManager.Instance.SetCursorPosition(screenPosition);
    }

    /// <summary>
    /// Writes a line to the console and to the log file in the output folder.
    /// </summary>
    /// <param name="message">The message to write.</param>
    public void Log(string message)
    {
        this._console?.WriteLine($"  · {message}");
        this._log?.WriteLine(message);
    }

    /// <summary>
    /// Renders the next frame and saves it as a numbered screenshot in the output folder.
    /// </summary>
    /// <param name="label">A short label included in the file name.</param>
    /// <param name="includeUi">Whether to keep the GUI, cursor, and tooltips, or capture only the 3D world.</param>
    /// <returns>A task that completes once the file is written.</returns>
    public async Task Capture(string label = "shot", bool includeUi = true)
    {
        // A screen changed this frame is not laid out and has no glyphs in the atlas until the next update.
        await this.NextFrame();

        var stage = this.App.Stage;
        var hiddenLayers = new List<ILayer2D>();
        if (!includeUi)
        {
            foreach (var layer in stage.Layers)
            {
                if (layer is ILayer2D layer2D && layer2D.IsVisible)
                {
                    layer2D.IsVisible = false;
                    hiddenLayers.Add(layer2D);
                }
            }
        }

        try
        {
            using var image = this._capturer!.Capture(stage);
            string fileName = $"{this._shots.Count:D2}_{label.Slug()}.png";
            image.SaveAsPng(Path.Combine(this.OutputDirectory, fileName));
            this._shots.Add(fileName);
            this.Log($"captured {fileName}");
        }
        finally
        {
            foreach (var layer in hiddenLayers)
            {
                layer.IsVisible = true;
            }
        }
    }

    /// <summary>
    /// Boots the application and deletes the screenshots of a previous run. <see cref="Dispose"/> quits it.
    /// </summary>
    /// <typeparam name="TSelf">The type of this scenario, handed back to the caller.</typeparam>
    /// <param name="createApp">Builds the application to drive, with its window size already set.</param>
    /// <param name="show">Whether to open a real window instead of running hidden.</param>
    /// <returns>An awaitable that yields this scenario on the game loop thread once the application is running.</returns>
    protected ScenarioStart<TSelf> Start<TSelf>(Func<Application> createApp, bool show) where TSelf : ScenarioBase
    {
        this.PrepareOutputDirectory();

        // The game's own console output goes to the log file, so the console only shows the script's Log lines.
        this._console = Console.Out;
        this._log = new StreamWriter(Path.Combine(this.OutputDirectory, "log.txt"), append: false) { AutoFlush = true };
        Console.SetOut(this._log);

        StartWatchdog();

        InputManager.UseVirtualInput = true;
        Application.Headless = !show;

        var start = new ScenarioStart<TSelf>((TSelf)this, this._loop);

        // The graphics context belongs to the thread that creates it, so the app is built on the loop thread too.
        var thread = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(this._loop);

            var app = createApp();
            this._app = app;
            app.Window.WindowState = show ? WindowState.Normal : WindowState.Hidden;

            // Advance the simulation by a fixed step each frame so animated content lands at the same phase every run.
            app.Ticker.FixedDeltaTime = 1.0 / 60.0;

            app.Ticker.Ticked += (sender, e) =>
            {
                if (this._capturer == null)
                {
                    this._capturer = new StageCapturer(app.Renderer);
                    InputManager.Instance.SetCursorPosition(new Vector2(-1, -1));
                    start.MarkStarted();
                }

                this._loop.Drain();
                this._scheduler.Tick(e.DeltaTime);
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
    /// Quits the application.
    /// </summary>
    public void Dispose()
    {
        if (this._app == null)
        {
            return;
        }

        // The capturer's GPU texture must go while the renderer is still alive.
        this._capturer?.Dispose();
        this._capturer = null;

        this._app.Quit();
        this._app = null;

        Console.SetOut(this._console!);
        this._log?.Dispose();
        this._log = null;

        Console.WriteLine($"  {this._shots.Count} shot(s) -> {this.OutputDirectory}");
    }

    private void PrepareOutputDirectory()
    {
        Directory.CreateDirectory(this.OutputDirectory);

        // The script itself lives in this folder, so only the shots go.
        foreach (string file in Directory.EnumerateFiles(this.OutputDirectory, "*.png"))
        {
            File.Delete(file);
        }
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
