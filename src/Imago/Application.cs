using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime;
using Imago.Diagnostics;
using Imago.Input;
using Imago.Metrics;
using Imago.Rendering;
using Imago.Support;
using Imago.Support.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NeoVeldrid;
using NeoVeldrid.Sdl2;
using NeoVeldrid.StartupUtilities;

namespace Imago;

/// <summary>
/// An application built on the Imago engine. Provides window management, rendering, input handling, and the game loop,
/// and runs a <see cref="Game"/>.
/// </summary>
/// <remarks>
/// Create one with <see cref="Configure{TGame}"/>.
/// </remarks>
public class Application : IDisposable
{
    /// <summary>
    /// Gets the singleton instance of the application.
    /// </summary>
    public static Application Instance { get; private set; } = null!;

    /// <summary>
    /// Gets a value indicating whether the application is running in debug mode.
    /// </summary>
#if DEBUG
    public static bool IsDebug => true;
#else
    public static bool IsDebug => false;
#endif

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
    /// Gets the screen manager that shows the screens on the stage.
    /// </summary>
    public ScreenManager Screens { get; }

    /// <summary>
    /// Gets the main viewport.
    /// </summary>
    public Viewport Viewport => this.Renderer.MainViewport;

    /// <summary>
    /// Gets the metrics of the application and its modules.
    /// </summary>
    public FrameMetrics Metrics { get; }

    /// <summary>
    /// Gets the services of the application and its modules.
    /// </summary>
    public IServiceProvider Services => this._services;

    private readonly ServiceProvider _services;

    private readonly ExceptionHandler _exceptions;

    private readonly GuiSizeMode _guiSizeMode;

    private readonly Game _game;

    private readonly ImagoMetrics _metrics;

    private long _allocatedBytes;

    private readonly int[] _collectionCounts = new int[3];

    /// <summary>
    /// Initializes a new instance of the <see cref="Application"/> class.
    /// </summary>
    /// <param name="builder">The settings of the application.</param>
    /// <param name="exceptions">The exception handler of the application. The application disposes it.</param>
    internal Application(ApplicationBuilder builder, ExceptionHandler exceptions)
    {
        Instance = this;
        this._exceptions = exceptions;

        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        this._guiSizeMode = builder.GuiSizeMode;

        this.Metrics = new FrameMetrics();
        this._metrics = this.Metrics.Imago;

        this.Window = CreateWindow(builder);
        this.Renderer = new Renderer(this.Window, this._metrics, builder.Backend);
        this.Input = new InputManager(this.Window);
        this.Ticker = new Ticker();
        this.Stage = new Stage();

        this.Window.Resized += this.Window_Resized;
        this.Ticker.Ticked += this.Ticker_Ticked;

        this.Window_Resized();
        this.Stage.EnableInputHandling();

        this._services = this.BuildServices(builder);
        this.Screens = this._services.GetRequiredService<ScreenManager>();

        foreach (var module in builder.Modules)
        {
            module.Boot(this);
        }

        this._game = this._services.GetRequiredService<Game>();

        this._allocatedBytes = GC.GetAllocatedBytesForCurrentThread();
        for (int generation = 0; generation < this._collectionCounts.Length; generation++)
        {
            this._collectionCounts[generation] = GC.CollectionCount(generation);
        }
    }

    /// <summary>
    /// Starts configuring an application that runs the given game.
    /// </summary>
    /// <typeparam name="TGame">The type of the game. The application creates it, so its constructor can take services as parameters.</typeparam>
    /// <returns>A builder for the application.</returns>
    public static ApplicationBuilder Configure<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TGame>()
        where TGame : Game
    {
        return new ApplicationBuilder(typeof(TGame));
    }

    private static Sdl2Window CreateWindow(ApplicationBuilder builder)
    {
        var info = new WindowCreateInfo
        {
            X = 100,
            Y = 100,
            WindowWidth = builder.WindowWidth,
            WindowHeight = builder.WindowHeight,
            WindowTitle = builder.Title,
            WindowInitialState = builder.IsHeadless ? WindowState.Hidden : builder.WindowState,
        };

        return NeoVeldridStartup.CreateWindow(info);
    }

    private ServiceProvider BuildServices(ApplicationBuilder builder)
    {
        var services = new ServiceCollection();
        services.AddSingleton(this);
        services.AddSingleton(this.Window);
        services.AddSingleton(this.Renderer);
        services.AddSingleton(this.Input);
        services.AddSingleton(this.Ticker);
        services.AddSingleton(this.Stage);
        services.AddSingleton(this.Metrics);
        services.AddSingleton(this._exceptions.LoggerFactory);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.AddSingleton(sp => new ScreenManager(sp, this, builder.Screens.Screens));
        services.AddSingleton<IScreenNavigator>(sp => sp.GetRequiredService<ScreenManager>());

        foreach (var module in builder.Modules)
        {
            module.Register(services);
        }

        foreach (var register in builder.ServiceRegistrations)
        {
            register(services);
        }

        services.AddSingleton(builder.GameType);
        services.AddSingleton(sp => (Game)sp.GetRequiredService(builder.GameType));

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Runs the main loop of the application. Returns once the application has been quit and fully disposed.
    /// </summary>
    /// <remarks>
    /// If the loop throws and no debugger is attached, the crash is reported and shown, and the process ends.
    /// </remarks>
    public void Run()
    {
        try
        {
            this.Ticker.Start();
        }
        catch (Exception e) when (!Debugger.IsAttached)
        {
            this._exceptions.Crash(e);
        }

        this.Dispose();
    }

    /// <summary>
    /// Signals the application to quit. The actual disposal happens once the current frame completes,
    /// so it is safe to call this from input handlers, GUI callbacks, or any other tick-time code.
    /// </summary>
    public virtual void Quit()
    {
        this.Ticker.Stop();
    }

    private void Window_Resized()
    {
        var size = new Vector2(this.Window.Width, this.Window.Height);
        this.Renderer.MainViewport.Resize(size);
        this.Renderer.Resize((uint)size.X, (uint)size.Y);

        var guiSize = this._guiSizeMode.ComputeSize(size);
        this.Renderer.GuiViewport.Resize(guiSize);
        this.Renderer.ResizeGui((uint)guiSize.X, (uint)guiSize.Y);
        this.Stage.GuiScale = size / guiSize;
    }

    private void Ticker_Ticked(object? sender, TickedEventArgs e)
    {
        float deltaTime = (float)e.DeltaTime;
        var metrics = this._metrics;

        metrics.FrameTime.Add(TimeSpan.FromSeconds(e.DeltaTime));

        using (metrics.UpdateTime.StartTimer())
        {
            this.Input.UpdateState();
            this.Renderer.Update(deltaTime);

            using (metrics.GameUpdateTime.StartTimer())
            {
                this._game.Update(deltaTime);
            }

            this.Screens.ShowRequestedScreen();
            this.Screens.Update(deltaTime);

            using (metrics.StageUpdateTime.StartTimer())
            {
                this.Stage.Update(deltaTime);
            }

            if (!this.Window.Exists)
            {
                this.Quit();
                return;
            }

            using (metrics.GameUpdateTime.StartTimer())
            {
                this._game.PostUpdate(deltaTime);
            }
        }

        this.Renderer.Render(this.Stage);

        this.RecordMemory();
        this.Metrics.EndFrame();
    }

    private void RecordMemory()
    {
        long allocatedBytes = GC.GetAllocatedBytesForCurrentThread();
        this._metrics.AllocatedBytes.Add((int)(allocatedBytes - this._allocatedBytes));
        this._allocatedBytes = allocatedBytes;

        this.RecordCollections(this._metrics.Gen0CollectionCount, 0);
        this.RecordCollections(this._metrics.Gen1CollectionCount, 1);
        this.RecordCollections(this._metrics.Gen2CollectionCount, 2);
    }

    private void RecordCollections(CounterMetric counter, int generation)
    {
        int count = GC.CollectionCount(generation);
        counter.Add(count - this._collectionCounts[generation]);
        this._collectionCounts[generation] = count;
    }

    /// <summary>
    /// Releases resources used by the application.
    /// </summary>
    public virtual void Dispose()
    {
        this.Ticker.Stop();
        this._services.Dispose();
        this.Input.Dispose();
        this.Renderer.Dispose();
        this.Window.Close();
        this._exceptions.Dispose();
    }
}
