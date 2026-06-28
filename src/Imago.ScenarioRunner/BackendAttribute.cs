using System;
using NeoVeldrid;

namespace Imago.ScenarioRunner;

/// <summary>
/// Overrides the graphics backend for a single scenario, taking precedence over the suite-wide default and the
/// <c>--backend</c> command-line option.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class BackendAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BackendAttribute"/> class.
    /// </summary>
    /// <param name="backend">The graphics backend the scenario runs on.</param>
    public BackendAttribute(GraphicsBackend backend)
    {
        this.Backend = backend;
    }

    /// <summary>
    /// Gets the graphics backend the scenario runs on.
    /// </summary>
    public GraphicsBackend Backend { get; }
}
