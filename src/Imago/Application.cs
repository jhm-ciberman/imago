using System;
using System.Diagnostics;
using System.Runtime;
using Imago.Input;
using Imago.Rendering;
using Imago.Resources;
using Imago.SceneGraph;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Viewport = Imago.SceneGraph.Viewport;

namespace Imago;

public class Application : IDisposable
{
    /// <summary>
    /// Gets the instance of the <see cref="Application"/>.
    /// </summary>
    public static Application Instance { get; private set; } = null!;

    /// <summary>
    /// Gets the <see cref="TexturePacker"/>.
    /// </summary>
    public TexturePacker TexturePacker { get; internal set; }

    /// <summary>
    /// Gets the <see cref="Viewport"/>.
    /// </summary>
    public Viewport Viewport { get; }

    /// <summary>
    /// Gets the <see cref="Sdl2Window"/>.
    /// </summary>
    public Sdl2Window Window { get; }

    /// <summary>
    /// Gets or sets the current <see cref="Scene"/>.
    /// </summary>
    public Scene? Scene
    {
        get => this._renderer.Scene;
        set => this._renderer.Scene = value;
    }

    /// <summary>
    /// Gets whether the application is currently running.
    /// </summary>
    public bool IsRunning { get; private set; } = false;

    private readonly InputManager _input;

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

        WindowCreateInfo windowCI = new WindowCreateInfo(100, 100, 1024, 600, WindowState.Normal, "Imago");
        this.Window = VeldridStartup.CreateWindow(ref windowCI);
        this.Viewport = new Viewport((uint)this.Window.Width, (uint)this.Window.Height);

        this._renderer = new Renderer(this.Window, backend);

        this.TexturePacker = new TexturePacker();

        this.Window.Resized += this.OnResize;

        this.OnResize();

        this._input = new InputManager(this.Window);
    }

    private void OnResize()
    {
        uint width = (uint) this.Window.Width;
        uint height = (uint) this.Window.Height;
        this.Viewport.Resize(width, height);
        this._renderer.Resize(width, height, this.Viewport.Width, this.Viewport.Height);
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
    public double DeltaTime { get; private set; }

    /// <summary>
    /// Gets the total time passed since the start of the application in seconds.
    /// </summary>
    public double ElapsedTime { get; private set; }

    /// <summary>
    /// Gets the number of frames per second.
    /// </summary>
    public double FramesPerSecond { get; private set; }

    protected virtual void OnUpdate()
    {
        // This can be overridden by derived classes.
    }

    /// <summary>
    /// Starts the application.
    /// </summary>
    public void Run()
    {
        if (this.IsRunning) return;

        this.IsRunning = true;

        Stopwatch sw = Stopwatch.StartNew();
        double previousElapsed = sw.Elapsed.TotalSeconds;

        while (this.IsRunning)
        {
            this._input.UpdateFrameInput();

            if (!this.Window.Exists)
            {
                this.IsRunning = false;
                break;
            }

            this.ElapsedTime = sw.Elapsed.TotalSeconds;
            this.DeltaTime = this.ElapsedTime - previousElapsed;
            previousElapsed = this.ElapsedTime;

            this.FramesPerSecond = 1d / this.DeltaTime;

            this._renderer.Update((float)this.DeltaTime, this._input.InputSnapshot);

            this.OnUpdate();

            this._renderer.Render();
        }
    }

    public void Dispose()
    {
        this._renderer.Dispose();
        this.Window.Close();
    }
}
