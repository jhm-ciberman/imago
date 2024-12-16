using System;
using System.Runtime;
using LifeSim.Imago.Rendering;
using LifeSim.Imago.Input;
using LifeSim.Imago.SceneGraph;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace LifeSim.Imago;

public class Application : IDisposable
{
    /// <summary>
    /// Gets the instance of the <see cref="Application"/>.
    /// </summary>
    public static Application Instance { get; private set; } = null!;

    /// <summary>
    /// Gets the <see cref="Sdl2Window"/>.
    /// </summary>
    public Sdl2Window Window { get; }

    /// <summary>
    /// Gets the main stage.
    /// </summary>
    public Stage Stage { get; } = new Stage();

    private readonly InputManager _input;

    /// <summary>
    /// Gets the <see cref="Ticker"/> in charge of running the main loop.
    /// </summary>
    public Ticker Ticker { get; } = new Ticker();

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

        this.Window = VeldridStartup.CreateWindow(new WindowCreateInfo
        {
            X = 100,
            Y = 100,
            WindowWidth = 1024,
            WindowHeight = 600,
            WindowTitle = "LifeSim.Imago",
            WindowInitialState = WindowState.Normal,
        });

        this._renderer = new Renderer(this.Window, backend);

        this.Window.Resized += this.Window_Resized;
        this.Ticker.Ticked += this.Ticker_Ticked;

        this.Window_Resized();

        this._input = new InputManager(this.Window);
    }

    public void Run()
    {
        this.Ticker.Start();
    }

    private void Window_Resized()
    {
        uint width = (uint) this.Window.Width;
        uint height = (uint) this.Window.Height;
        this._renderer.Viewport.Resize(width, height);
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

    private void Ticker_Ticked(object? sender, TickedEventArgs e)
    {
        this._input.UpdateFrameInput();

        this._renderer.Update((float)this.Ticker.DeltaTime, this._input.InputSnapshot);

        this.Update();

        this.Stage.Update((float)this.Ticker.DeltaTime);

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
        this.Ticker.Stop();
        this._input.Dispose();
        this._renderer.Dispose();
        this.Window.Close();
    }
}
