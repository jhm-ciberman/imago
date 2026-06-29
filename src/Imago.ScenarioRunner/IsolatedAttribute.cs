using System;

namespace Imago.ScenarioRunner;

/// <summary>
/// Marks a scenario to run in its own process instead of sharing one with other scenarios. Use it for a
/// scenario that crashes the process or that cannot be reached from a clean baseline by normal navigation.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class IsolatedAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IsolatedAttribute"/> class.
    /// </summary>
    /// <param name="reason">Why the scenario must run alone, for whoever reads the code later.</param>
    public IsolatedAttribute(string? reason = null)
    {
        this.Reason = reason;
    }

    /// <summary>
    /// Gets the reason the scenario must run in its own process, or null when none was given.
    /// </summary>
    public string? Reason { get; }
}
