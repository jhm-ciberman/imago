using System;
using System.Threading.Tasks;

namespace Imago.ScenarioRunner;

/// <summary>
/// Runs a single scenario to completion, then quits the application.
/// </summary>
internal sealed class ScenarioDriver : IDisposable
{
    private readonly Application _app;
    private readonly ScenarioDescriptor _scenario;
    private readonly ScenarioScheduler _scheduler = new();
    private readonly ScenarioContext _context;

    private Task? _task;
    private bool _finished;

    public ScenarioDriver(Application app, ScenarioDescriptor scenario, string outputDirectory)
    {
        this._app = app;
        this._scenario = scenario;
        this._context = new ScenarioContext(app, this._scheduler, outputDirectory, scenario.Name);
    }

    public int ExitCode { get; private set; }

    public void Tick(float deltaTime)
    {
        this._task ??= this._scenario.Run(this._context);
        this._scheduler.Tick(deltaTime);

        if (this._finished || !this._task.IsCompleted)
        {
            return;
        }

        this._finished = true;

        if (this._task.IsFaulted)
        {
            this.ExitCode = 1;
            Console.Error.WriteLine($"Scenario '{this._scenario.Name}' failed: {this._task.Exception?.GetBaseException()}");
        }

        // Release the capturer's GPU texture while the renderer is still alive, before Run() tears it down.
        this._context.Dispose();
        this._app.Quit();
    }

    public void Dispose()
    {
        this._context.Dispose();
    }
}
