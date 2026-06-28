using System;
using Imago.Rendering;

namespace Imago.ScenarioRunner;

/// <summary>
/// The resources for a single scenario run.
/// </summary>
internal sealed class ScenarioContext : IDisposable
{
    private bool _disposed;

    public ScenarioContext(Application app, ScenarioScheduler scheduler, string outputDirectory, string name)
    {
        this.App = app;
        this.Scheduler = scheduler;
        this.Capturer = new StageCapturer(app.Renderer);
        this.OutputDirectory = outputDirectory;
        this.Name = name;
    }

    public Application App { get; }

    public ScenarioScheduler Scheduler { get; }

    public StageCapturer Capturer { get; }

    public string OutputDirectory { get; }

    public string Name { get; }

    public void Dispose()
    {
        if (this._disposed)
        {
            return;
        }

        this._disposed = true;
        this.Capturer.Dispose();
    }
}
