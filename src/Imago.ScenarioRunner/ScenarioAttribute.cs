using System;

namespace Imago.ScenarioRunner;

/// <summary>
/// Marks a method as a runnable scenario. The method must be public, take no parameters, return a
/// <see cref="System.Threading.Tasks.Task"/>, and belong to a non-abstract <see cref="ScenarioBase"/> subclass.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ScenarioAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScenarioAttribute"/> class.
    /// </summary>
    /// <param name="description">A one-line description of what the scenario captures.</param>
    public ScenarioAttribute(string? description = null)
    {
        this.Description = description;
    }

    /// <summary>
    /// Gets the one-line description of what the scenario captures, or null when none was given.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets or sets the command-line name. Defaults to the method name in kebab-case when null.
    /// </summary>
    public string? Name { get; init; }
}
