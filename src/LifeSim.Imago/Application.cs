using System;
using System.Runtime;
using LifeSim.Imago.Rendering;
using LifeSim.Imago.Input;
using LifeSim.Imago.SceneGraph;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Viewport = LifeSim.Imago.SceneGraph.Viewport;

namespace LifeSim.Imago;

public class Application : IDisposable
{
    /// <summary>
    /// Gets the instance of the <see cref="Application"/>.
    /// </summary>
    public static Application Instance { get; private set; } = null!;

    /// <summary>
    /// Gets the <see cref="Viewport"/>.
    /// </summary>
    public Viewport Viewport => this._renderer.Viewport;

    /// <summary>
    /// Gets the <see cref="Sdl2Window"/>.
    /// </summary>
    public Sdl2Window Window { get; }

    /// <summary>
    /// Gets the main stage.
    /// </summary>
    public Stage Stage { get; } = new Stage();

    private readonly InputManager _input;

    private readonly Ticker _ticker = new Ticker();

    private readonly Renderer _renderer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Application"/> class.
    /// </summary>
    /// <param name="backend">The graphics backend to use.</param>
    /// <exception cref="InvalidOperationException">Thrown if an instance of <see cref="Application"/> already exists.</exception>
    public Application(GraphicsBackend? backend = null)
    {
        if (Instance != null)
        {
            throw new InvalidOperationException("Only one instance of Application can exist.");
        }

        Instance = this;

        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        WindowCreateInfo windowCI = new WindowCreateInfo(100, 100, 1024, 600, WindowState.Normal, "LifeSim.Imago");
        this.Window = VeldridStartup.CreateWindow(ref windowCI);

        this._renderer = new Renderer(this.Window, backend);

        this.Window.Resized += this.OnResize;
        this._ticker.Tick += this.Ticker_Tick;

        this.OnResize();

        this._input = new InputManager(this.Window);
    }

    public void Run()
    {
        this._ticker.Start();
    }

    private void OnResize()
    {
        uint width = (uint) this.Window.Width;
        uint height = (uint) this.Window.Height;
        this.Viewport.Resize(width, height);
        this._renderer.Resize(width, height);
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void Quit()
    {
        this.Dispose();
    }

    /// <summary>
    /// Gets the time passed since the last frame in seconds.
    /// </summary>
    public double DeltaTime => this._ticker.DeltaTime;

    /// <summary>
    /// Gets the total time passed since the start of the application in seconds.
    /// </summary>
    public double ElapsedTime => this._ticker.ElapsedTime;

    /// <summary>
    /// Gets the number of frames per second.
    /// </summary>
    public double FramesPerSecond => this._ticker.FramesPerSecond;

    /// <summary>
    /// Gets or sets the minimum time that should pass between frames.
    /// </summary>
    public float TargetFrameTime
    {
        get => this._ticker.TargetFrameTime;
        set => this._ticker.TargetFrameTime = value;
    }

    /// <summary>
    /// Gets or sets the target frames per second.
    /// </summary>
    public float TargetFramesPerSecond
    {
        get => 1f / this.TargetFrameTime;
        set => this.TargetFrameTime = 1f / value;
    }

    /// <summary>
    /// Gets a value indicating whether the application is running.
    /// </summary>
    public bool IsRunning => this._ticker.IsRunning;

    /// <summary>
    /// Gets the current <see cref="Scene"/>.
    /// </summary>
    public Scene Scene => this.Stage.Scene;

    protected virtual void Update()
    {
        // This can be overridden by derived classes.
    }

    protected virtual void PrepareForRender()
    {
        // This can be overridden by derived classes.
    }

    public void ChangeScene(Scene scene)
    {
        this.Stage.ChangeScene(scene);
    }

    private void Ticker_Tick(object? sender, TickEventArgs e)
    {
        this._input.UpdateFrameInput();

        this._renderer.Update((float)this.DeltaTime, this._input.InputSnapshot);

        this.Update();

        this.Stage.Update((float)this.DeltaTime);

        if (!this.Window.Exists)
        {
            this.Quit();
            return;
        }

        this.PrepareForRender();

        this._renderer.Render(this.Stage);
    }

    public void Dispose()
    {
        this._ticker.Stop();
        this._input.Dispose();
        this._renderer.Dispose();
        this.Window.Close();
    }
}
