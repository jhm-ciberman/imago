using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Imago.Input;

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
    private readonly Action? _reset;
    private readonly (int Width, int Height)? _defaultWindowSize;

    private int _index;
    private ScenarioScheduler? _scheduler;
    private ScenarioContext? _context;
    private Task? _task;
    private bool _finished;
    private (int Width, int Height)? _windowSize;

    public ScenarioDriver(
        Application app,
        IReadOnlyList<ScenarioDescriptor> scenarios,
        Action? reset,
        (int Width, int Height)? defaultWindowSize)
    {
        this._app = app;
        this._scenarios = scenarios;
        this._reset = reset;
        this._defaultWindowSize = defaultWindowSize;
        this._windowSize = defaultWindowSize;
    }

    public int ExitCode { get; private set; }

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

        Console.WriteLine($"Running scenario '{scenario.Name}': {scenario.Description}");
        this.ApplyWindowSize(scenario);

        this._scheduler = new ScenarioScheduler();
        this._context = new ScenarioContext(this._app, this._scheduler, outputDirectory, scenario.Name);
        this._task = scenario.Run(this._context);
    }

    private void FinishCurrent()
    {
        var scenario = this._scenarios[this._index];
        string outputDirectory = this._context!.OutputDirectory;

        if (this._task!.IsFaulted)
        {
            this.ExitCode = 1;
            Console.Error.WriteLine($"Scenario '{scenario.Name}' failed: {this._task.Exception?.GetBaseException()}");
        }
        else
        {
            Console.WriteLine($"  Executed. Review the screenshots in {Path.GetFullPath(outputDirectory)}");
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
        var target = scenario.WindowSize ?? this._defaultWindowSize;
        if (target is not { } size || this._windowSize == size)
        {
            return;
        }

        this._app.Window.Width = size.Width;
        this._app.Window.Height = size.Height;
        this._windowSize = size;
    }

    public void Dispose()
    {
        this._context?.Dispose();
    }
}
