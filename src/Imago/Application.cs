using System;
using System.Numerics;
using System.Runtime;
using Imago.Input;
using Imago.Rendering;
using Imago.SceneGraph;
using NeoVeldrid;
using NeoVeldrid.Sdl2;
using NeoVeldrid.StartupUtilities;

namespace Imago;

/// <summary>
/// Base class for applications built on the Imago engine.
/// Provides window management, rendering, input handling, and game loop infrastructure.
/// </summary>
public abstract class Application : IDisposable
{
    /// <summary>
    /// Gets the singleton instance of the application.
    /// </summary>
    public static Application Instance { get; private set; } = null!;

    /// <summary>
    /// Gets the SDL2 window.
    /// </summary>
    public Sdl2Window Window { get; }

    /// <summary>
    /// Gets the renderer.
    /// </summary>
    public Renderer Renderer { get; }

    /// <summary>
    /// Gets the input manager.
    /// </summary>
    public InputManager Input { get; }

    /// <summary>
    /// Gets the ticker that drives the main loop.
    /// </summary>
    public Ticker Ticker { get; }

    /// <summary>
    /// Gets the main stage that manages scenes and layers.
    /// </summary>
    public Stage Stage { get; }

    /// <summary>
    /// Gets the main viewport.
    /// </summary>
    public SceneGraph.Viewport Viewport => this.Renderer.MainViewport;


    /// <summary>
    /// Gets a value indicating whether the application is running in debug mode.
    /// </summary>
#if DEBUG
    public static bool IsDebug => true;
#else
    public static bool IsDebug => false;
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="Application"/> class.
    /// </summary>
    /// <param name="backend">The graphics backend to use. If null, the default backend is used.</param>
    /// <exception cref="InvalidOperationException">Thrown if an application instance already exists.</exception>
    protected Application(GraphicsBackend? backend = null)
    {
        if (Instance != null)
        {
            throw new InvalidOperationException("Only one instance of Application can exist.");
        }

        Instance = this;

        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        this.Window = this.CreateWindow();
        this.Renderer = new Renderer(this.Window, backend);
        this.Input = new InputManager(this.Window);
        this.Ticker = new Ticker();
        this.Stage = new Stage();

        this.Window.Resized += this.HandleWindowResized;
        this.Ticker.Ticked += this.HandleTicked;

        this.HandleWindowResized();
        this.Stage.EnableInputHandling();
    }

    /// <summary>
    /// Creates the application window. Override to customize window settings.
    /// </summary>
    /// <returns>The created window.</returns>
    protected virtual Sdl2Window CreateWindow()
    {
        return NeoVeldridStartup.CreateWindow(new WindowCreateInfo
        {
            X = 100,
            Y = 100,
            WindowWidth = 1024,
            WindowHeight = 600,
            WindowTitle = "Application",
            WindowInitialState = IsDebug ? WindowState.Maximized : WindowState.Normal,
        });
    }

    /// <summary>
    /// Gets the GUI sizing mode. Override to opt in to a fixed virtual resolution for pixel art or retro-style rendering.
    /// </summary>
    protected virtual GuiSizeMode GuiSizeMode => GuiSizeMode.Native;

    /// <summary>
    /// Runs the application main loop.
    /// </summary>
    public void Run()
    {
        this.Ticker.Start();
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public virtual void Quit()
    {
        this.Dispose();
    }

    private void HandleWindowResized()
    {
        var size = new Vector2(this.Window.Width, this.Window.Height);
        this.Renderer.MainViewport.Resize(size);
        this.Renderer.Resize((uint)size.X, (uint)size.Y);

        var guiSize = this.GuiSizeMode.ComputeSize(size);
        this.Renderer.GuiViewport.Resize(guiSize);
        this.Renderer.ResizeGui((uint)guiSize.X, (uint)guiSize.Y);
        this.Stage.GuiScale = size / guiSize;

        this.OnWindowResized();
    }

    private void HandleTicked(object? sender, TickedEventArgs e)
    {
        float deltaTime = (float)e.DeltaTime;

        this.Renderer.Statistics.UpdateTime.Begin();

        this.Input.UpdateState();
        this.Renderer.Update(deltaTime, this.Input.InputSnapshot);

        this.OnUpdate(deltaTime);

        this.Stage.Update(deltaTime);

        if (!this.Window.Exists)
        {
            this.Quit();
            return;
        }

        this.OnPostUpdate(deltaTime);

        this.UpdateWindowTitle();

        this.Renderer.Statistics.UpdateTime.End();

        this.Renderer.Render(this.Stage);
    }

    /// <summary>
    /// Called when the window is resized. Override to handle resize events.
    /// </summary>
    protected virtual void OnWindowResized()
    {
    }

    /// <summary>
    /// Called each frame before the stage update. Override to implement frame logic.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last frame in seconds.</param>
    protected virtual void OnUpdate(float deltaTime)
    {
    }

    /// <summary>
    /// Called each frame after the stage update but before rendering.
    /// Override to flush visual changes or perform post-update logic.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last frame in seconds.</param>
    protected virtual void OnPostUpdate(float deltaTime)
    {
    }

    /// <summary>
    /// Updates the window title. Override to customize the title bar content.
    /// </summary>
    protected virtual void UpdateWindowTitle()
    {
        if (!this.Window.Exists)
        {
            return;
        }

        var dt = this.Ticker.DeltaTime * 1000;
        var fps = this.Ticker.FramesPerSecond;
        this.Window.Title = $"Application ({this.Renderer.BackendType}) {dt:0.00}ms | {fps:0.00} FPS";
    }

    /// <summary>
    /// Releases resources used by the application.
    /// </summary>
    public virtual void Dispose()
    {
        this.Ticker.Stop();
        this.Input.Dispose();
        this.Renderer.Dispose();
        this.Window.Close();
    }
}
