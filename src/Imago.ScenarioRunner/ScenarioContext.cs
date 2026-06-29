using System;
using System.Collections.Generic;
using Imago.Rendering;

namespace Imago.ScenarioRunner;

/// <summary>
/// The resources for a single scenario run.
/// </summary>
internal sealed class ScenarioContext : IDisposable
{
    private bool _disposed;

    public ScenarioContext(Application app, ScenarioScheduler scheduler, ConsolePresenter presenter, string outputDirectory, string name)
    {
        this.App = app;
        this.Scheduler = scheduler;
        this.Presenter = presenter;
        this.Capturer = new StageCapturer(app.Renderer);
        this.OutputDirectory = outputDirectory;
        this.Name = name;
    }

    public Application App { get; }

    public ScenarioScheduler Scheduler { get; }

    public ConsolePresenter Presenter { get; }

    public StageCapturer Capturer { get; }

    public string OutputDirectory { get; }

    public string Name { get; }

    /// <summary>
    /// Gets the labels of the shots captured so far, in capture order.
    /// </summary>
    public List<string> Shots { get; } = new();

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
