using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Imago.Rendering;
using Imago.Support;
using Microsoft.Extensions.DependencyInjection;
using NeoVeldrid;
using SixLabors.ImageSharp;

namespace Imago.Scenarios;

/// <summary>
/// Drives a running application from a script and saves the screenshots it captures. A game derives from this to
/// add helpers for its own screens and state.
/// </summary>
/// <remarks>
/// Start one with <see cref="StartAsync{TScenario}"/>. Every await in the script resumes on the game loop thread,
/// so the script can touch the game freely.
/// </remarks>
public abstract class Scenario : IDisposable
{
    private const double DefaultTimeoutSeconds = 20.0;

    private readonly ScenarioScheduler _scheduler = new();
    private readonly List<string> _shots = new();
    private readonly Dictionary<Type, object> _tools = new();

    private ScenarioHost? _host;
    private Application? _app;
    private StageCapturer? _capturer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Scenario"/> class.
    /// </summary>
    protected Scenario()
    {
        this.Mouse = new ScenarioMouse(this);
        this.Keyboard = new ScenarioKeyboard(this);
        this.Window = new ScenarioWindow(this);
    }

    /// <summary>
    /// Gets the folder the screenshots and the log are written to.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no run is in progress.</exception>
    public string OutputDirectory => this.Host.OutputDirectory;

    /// <summary>
    /// Gets the application being driven.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no run is in progress.</exception>
    public Application App => this._app ?? throw new InvalidOperationException("No run is in progress.");

    /// <summary>
    /// Gets the virtual mouse, which clicks and moves like a player's mouse.
    /// </summary>
    public ScenarioMouse Mouse { get; }

    /// <summary>
    /// Gets the virtual keyboard, which presses keys and types like a player's keyboard.
    /// </summary>
    public ScenarioKeyboard Keyboard { get; }

    /// <summary>
    /// Gets the window of the application being driven.
    /// </summary>
    public ScenarioWindow Window { get; }

    private ScenarioHost Host => this._host ?? throw new InvalidOperationException("No run is in progress.");

    /// <summary>
    /// Creates the application on its own thread, drives it with a new scenario of the given type, and waits until
    /// it runs.
    /// </summary>
    /// <remarks>
    /// The backend and the window come from the arguments, then from the command line (<c>--backend</c> and
    /// <c>--show</c>). By default the scenario runs on OpenGL with a hidden window. The screenshots and the log go to
    /// the folder of the calling script, and disposing the scenario quits the application.
    /// </remarks>
    /// <typeparam name="TScenario">The type of the scenario.</typeparam>
    /// <param name="backend">The graphics backend, or null to read it from the command line.</param>
    /// <param name="show">Whether to open a real window instead of running hidden.</param>
    /// <param name="scriptPath">Filled in by the compiler with the path of the calling script.</param>
    /// <returns>An awaitable that yields the running scenario on the game loop thread.</returns>
    /// <exception cref="ArgumentException">Thrown if the command line names an unknown backend.</exception>
    public static ScenarioStart<TScenario> StartAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TScenario>(
        GraphicsBackend? backend = null,
        bool show = false,
        [CallerFilePath] string scriptPath = ""
    )
        where TScenario : Scenario, new()
    {
        var scenario = new TScenario();
        return ScenarioHost.Start(scenario, scenario.Bootstrap(), backend, show, Path.GetDirectoryName(scriptPath)!);
    }

    /// <summary>
    /// Sets up the application that the scenario drives.
    /// </summary>
    /// <returns>A builder for the application.</returns>
    protected abstract ApplicationBuilder Bootstrap();

    /// <summary>
    /// Gets a service of the running application.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <returns>The service.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no run is in progress or the service is not registered.</exception>
    public T Service<T>() where T : notnull
    {
        return this.App.Services.GetRequiredService<T>();
    }

    /// <summary>
    /// Adds a tool to this scenario, so it can be found with <see cref="Tool{T}"/>.
    /// </summary>
    /// <remarks>
    /// Modules call this from <see cref="Module.BootScenario"/> and expose the tool with an extension
    /// property, so scripts do not call <see cref="Tool{T}"/> themselves.
    /// </remarks>
    /// <typeparam name="T">The type the tool is found by.</typeparam>
    /// <param name="tool">The tool to add.</param>
    /// <exception cref="InvalidOperationException">Thrown if a tool of the same type was already added.</exception>
    public void AddTool<T>(T tool) where T : class
    {
        if (!this._tools.TryAdd(typeof(T), tool))
        {
            throw new InvalidOperationException($"A {typeof(T).Name} tool was already added.");
        }
    }

    /// <summary>
    /// Gets a tool that a module added to this scenario.
    /// </summary>
    /// <typeparam name="T">The type of the tool.</typeparam>
    /// <returns>The tool.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no tool of that type was added.</exception>
    public T Tool<T>() where T : class
    {
        if (this._tools.TryGetValue(typeof(T), out var tool))
        {
            return (T)tool;
        }

        throw new InvalidOperationException($"No {typeof(T).Name} tool was added. Add the module that provides it to the application.");
    }

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
    /// Writes a line to the console and to the log file in the output folder.
    /// </summary>
    /// <param name="message">The message to write.</param>
    public void Log(string message)
    {
        this._host?.Log(message);
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

        using var image = this._capturer!.Capture(this.App.Stage, includeUi: includeUi);
        string fileName = $"{this._shots.Count:D2}_{label.Slug()}.png";
        image.SaveAsPng(Path.Combine(this.OutputDirectory, fileName));
        this._shots.Add(fileName);
        this.Log($"captured {fileName}");
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

        var host = this.Host;
        this._host = null;

        Console.WriteLine($"  {this._shots.Count} shot(s) -> {host.OutputDirectory}");
    }

    internal void Attach(Application app, ScenarioHost host)
    {
        this._app = app;
        this._host = host;
        this._capturer = new StageCapturer(app.Renderer);
        app.Input.SetCursorPosition(new Vector2(-1, -1));
    }

    internal void Tick(double deltaTime)
    {
        this._scheduler.Tick(deltaTime);
    }
}
