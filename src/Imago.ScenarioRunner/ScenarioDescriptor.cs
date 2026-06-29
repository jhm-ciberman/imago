using System;
using System.Threading.Tasks;
using NeoVeldrid;

namespace Imago.ScenarioRunner;

/// <summary>
/// A discovered scenario: its name, description, the delegate that runs it, and any per-scenario overrides read
/// from its attributes.
/// </summary>
internal sealed class ScenarioDescriptor
{
    public ScenarioDescriptor(
        string name,
        string description,
        Func<ScenarioContext, Task> run,
        (int Width, int Height)? windowSize,
        GraphicsBackend? backend,
        bool isolated)
    {
        this.Name = name;
        this.Description = description;
        this.Run = run;
        this.WindowSize = windowSize;
        this.Backend = backend;
        this.Isolated = isolated;
    }

    public string Name { get; }

    public string Description { get; }

    public Func<ScenarioContext, Task> Run { get; }

    public (int Width, int Height)? WindowSize { get; }

    public GraphicsBackend? Backend { get; }

    public bool Isolated { get; }
}
