using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Imago.Input;
using Imago.Support.Numerics;

namespace Imago.ScenarioRunner;

/// <summary>
/// Runs a sequence of scenarios in one application, returning to a clean baseline between each, then quits.
/// </summary>
/// <remarks>
/// The first scenario starts from the freshly booted game. Between scenarios the driver parks the virtual
/// cursor off-screen and invokes the host's reset, so each scenario begins the way a tester would: back at a
/// neutral screen, walking the game's own navigation. Anything the previous scenario leaves behind is the
/// game's own teardown to handle, the same as it would be for a player.
/// </remarks>
internal sealed class ScenarioDriver : IDisposable
{
    private readonly Application _app;
    private readonly IReadOnlyList<ScenarioDescriptor> _scenarios;
    private readonly ConsolePresenter _presenter;
    private readonly Action? _reset;
    private readonly Stopwatch _stopwatch = new();

    private int _index;
    private ScenarioScheduler? _scheduler;
    private ScenarioContext? _context;
    private Task? _task;
    private bool _finished;
    private Vector2Int? _baselineWindowSize;
    private Vector2Int _windowSize;

    public ScenarioDriver(Application app, IReadOnlyList<ScenarioDescriptor> scenarios, ConsolePresenter presenter, Action? reset)
    {
        this._app = app;
        this._scenarios = scenarios;
        this._presenter = presenter;
        this._reset = reset;
    }

    /// <summary>
    /// Gets the number of scenarios that passed.
    /// </summary>
    public int Passed { get; private set; }

    /// <summary>
    /// Gets the number of scenarios that failed.
    /// </summary>
    public int Failed { get; private set; }

    /// <summary>
    /// Gets the total number of shots captured across all scenarios.
    /// </summary>
    public int Shots { get; private set; }

    /// <summary>
    /// Gets the size the booted window settled at, which scenarios render and capture at unless they override it.
    /// </summary>
    public Vector2Int WindowSize => this._baselineWindowSize ?? Vector2Int.Zero;

    public void Tick(float deltaTime)
    {
        if (this._finished)
        {
            return;
        }

        if (this._task == null)
        {
            this.StartCurrent();
        }

        this._scheduler!.Tick(deltaTime);

        if (this._task!.IsCompleted)
        {
            this.FinishCurrent();
        }
    }

    private void StartCurrent()
    {
        var scenario = this._scenarios[this._index];
        string outputDirectory = Path.Combine("artifacts", "scenarios", scenario.Name);
        Directory.CreateDirectory(outputDirectory);

        this.ApplyWindowSize(scenario);

        this._scheduler = new ScenarioScheduler();
        this._context = new ScenarioContext(this._app, this._scheduler, this._presenter, outputDirectory, scenario.Name);
        this._stopwatch.Restart();
        this._task = scenario.Run(this._context);
    }

    private void FinishCurrent()
    {
        double seconds = this._stopwatch.Elapsed.TotalSeconds;
        var scenario = this._scenarios[this._index];
        var shots = this._context!.Shots;
        this.Shots += shots.Count;

        if (this._task!.IsFaulted)
        {
            this.Failed++;
            var error = this._task.Exception?.GetBaseException() ?? new Exception("Unknown error.");
            this._presenter.ScenarioFailed(scenario.Name, seconds, error);
            Console.Error.WriteLine(error);
        }
        else
        {
            this.Passed++;
            this._presenter.ScenarioPassed(scenario.Name, shots, seconds);
        }

        // Release the capturer's GPU texture while the renderer is still alive, before the run is torn down.
        this._context.Dispose();
        this._context = null;
        this._scheduler = null;
        this._task = null;

        this._index++;
        if (this._index >= this._scenarios.Count)
        {
            this._finished = true;
            this._app.Quit();
            return;
        }

        this.ResetToBaseline();
    }

    private void ResetToBaseline()
    {
        // The virtual cursor is the tester's hand, not game state: park it off-screen, the inert state every
        // run starts from. Everything that is game state is left for the host's reset and the game's own teardown.
        InputManager.Instance.SetCursorPosition(new Vector2(-1, -1));
        this._reset?.Invoke();
    }

    private void ApplyWindowSize(ScenarioDescriptor scenario)
    {
        // The size the host gave the booted window is the baseline; a scenario's own size overrides it and the
        // next scenario without one falls back to it.
        if (this._baselineWindowSize is not { } baseline)
        {
            baseline = new Vector2Int(this._app.Window.Width, this._app.Window.Height);
            this._baselineWindowSize = baseline;
            this._windowSize = baseline;
        }

        var target = scenario.WindowSize ?? baseline;
        if (this._windowSize == target)
        {
            return;
        }

        this._app.Window.Width = target.X;
        this._app.Window.Height = target.Y;
        this._windowSize = target;
    }

    public void Dispose()
    {
        this._context?.Dispose();
    }
}
