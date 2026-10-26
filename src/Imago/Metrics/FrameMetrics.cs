using System;
using System.Collections.Generic;
using Imago.Support.Metrics;

namespace Imago.Metrics;

/// <summary>
/// Holds the metric groups of an application. Every metric records one value per frame.
/// </summary>
public sealed class FrameMetrics
{
    private readonly List<MetricGroup> _groups = [];
    private readonly Dictionary<Type, MetricGroup> _groupsByType = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="FrameMetrics"/> class.
    /// </summary>
    internal FrameMetrics()
    {
        this.Imago = this.Add(new ImagoMetrics());
    }

    /// <summary>
    /// Gets the metrics of the application loop and the renderer.
    /// </summary>
    public ImagoMetrics Imago { get; }

    /// <summary>
    /// Gets the metric groups, in the order they were added.
    /// </summary>
    public IReadOnlyList<MetricGroup> Groups => this._groups;

    /// <summary>
    /// Adds a metric group.
    /// </summary>
    /// <typeparam name="T">The type of the group.</typeparam>
    /// <param name="group">The group to add.</param>
    /// <returns>The added group.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a group of the same type was already added.</exception>
    public T Add<T>(T group) where T : MetricGroup
    {
        if (!this._groupsByType.TryAdd(typeof(T), group))
        {
            throw new InvalidOperationException($"A {typeof(T).Name} group was already added.");
        }

        this._groups.Add(group);
        return group;
    }

    /// <summary>
    /// Gets the metric group of the given type.
    /// </summary>
    /// <typeparam name="T">The type of the group.</typeparam>
    /// <returns>The group.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no group of that type was added.</exception>
    public T Group<T>() where T : MetricGroup
    {
        if (this._groupsByType.TryGetValue(typeof(T), out var group))
        {
            return (T)group;
        }

        throw new InvalidOperationException($"No {typeof(T).Name} group was added. Add the module that provides it to the application.");
    }

    internal void EndFrame()
    {
        foreach (var group in this._groups)
        {
            group.EndFrame();
        }
    }
}
