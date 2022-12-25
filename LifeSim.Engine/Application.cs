using System;
using System.Diagnostics;
using System.Runtime;
using LifeSim.Engine;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.Resources;
using LifeSim.Engine.SceneGraph;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Viewport = LifeSim.Engine.Rendering.Viewport;

namespace LifeSim.Engine;

public class Application : IDisposable
{
    /// <summary>
    /// Gets the current <see cref="Application"/> instance.
    /// </summary>
    public static Application Current { get; private set; } = null!;

    /// <summary>
    /// Gets the <see cref="Viewport"/> used by the <see cref="Application"/>.
    /// </summary>
    public Viewport Viewport { get; }

    /// <summary>
    /// Gets the <see cref="Sdl2Window"/> used by the <see cref="Application"/>.
    /// </summary>
    public Sdl2Window Window { get; }

    private readonly InputManager _input;

    private readonly Renderer _renderer;

    /// <summary>
    /// Gets or sets the current <see cref="Scene"/>.
    /// </summary>
    public Scene? Scene { get; set; } = null;

    public TexturePacker TexturePacker { get; internal set; }

    private bool _running = false;

    public Application(string windowTitle, GraphicsBackend? backend = null)
    {
        if (Current != null)
        {
            throw new InvalidOperationException("Only one instance of App can be created.");
        }

        Current = this;

        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        WindowCreateInfo windowCI = new WindowCreateInfo(100, 100, 1024, 600, WindowState.Normal, windowTitle);
        this.Window = VeldridStartup.CreateWindow(ref windowCI);
        this.Viewport = new Viewport((uint)this.Window.Width, (uint)this.Window.Height);

        this._renderer = new Renderer(this.Window, backend);

        this.TexturePacker = new TexturePacker(1024, 32);
        this._renderer.RenderStarted += this.OnRenderStarted;

        this.Window.Resized += this.OnResize;

        this.OnResize();

        this._input = new InputManager(this.Window);
    }

    private void OnRenderStarted(object? sender, EventArgs e)
    {
        this.TexturePacker.FlushChanges();
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
    /// <param name="scene">The initial scene to start with.</param>
    public void Start(Scene scene)
    {
        this.Scene = scene;
        this.Start();
    }

    /// <summary>
    /// Starts the application.
    /// </summary>
    public void Start()
    {
        if (this._running) return;

        this._running = true;

        Stopwatch sw = Stopwatch.StartNew();
        double previousElapsed = sw.Elapsed.TotalSeconds;

        while (this.Window.Exists)
        {
            this.ElapsedTime = sw.Elapsed.TotalSeconds;
            this.DeltaTime = this.ElapsedTime - previousElapsed;
            previousElapsed = this.ElapsedTime;

            this.FramesPerSecond = 1d / this.DeltaTime;

            this.OnUpdate();

            var scene = this.Scene;
            if (scene != null)
            {
                scene.Update((float)this.DeltaTime);

                this._renderer.Update((float)this.DeltaTime, this._input.InputSnapshot);

                try
                {
                    this._renderer.Render(scene);
                }
                catch (VeldridException e)
                {
                    Console.WriteLine(e.Message);

#if DEBUG
                    throw;
#endif
                }
            }

            this._input.UpdateFrameInput(); // For next frame
        }
    }

    public void Dispose()
    {
        this._renderer.Dispose();
        this.Window.Close();
    }
}