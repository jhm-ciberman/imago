using System.Collections.Generic;

namespace Imago.Support.Metrics;

/// <summary>
/// The metrics of one part of an application, like a module. Add it to the frame metrics of the application to record its metrics.
/// </summary>
public abstract class MetricGroup
{
    private readonly List<Metric> _all = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricGroup"/> class.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    protected MetricGroup(string name)
    {
        this.Name = name;
    }

    /// <summary>
    /// Gets the name of the group.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the metrics of the group, in the order they were created.
    /// </summary>
    public IReadOnlyList<Metric> All => this._all;

    /// <summary>
    /// Creates a timer metric in this group.
    /// </summary>
    /// <param name="name">The name of the metric.</param>
    /// <returns>The new metric.</returns>
    protected TimerMetric Timer(string name)
    {
        return this.Add(new TimerMetric(name));
    }

    /// <summary>
    /// Creates a counter metric in this group.
    /// </summary>
    /// <param name="name">The name of the metric.</param>
    /// <returns>The new metric.</returns>
    protected CounterMetric Counter(string name)
    {
        return this.Add(new CounterMetric(name));
    }

    /// <summary>
    /// Creates a gauge metric in this group.
    /// </summary>
    /// <param name="name">The name of the metric.</param>
    /// <returns>The new metric.</returns>
    protected GaugeMetric Gauge(string name)
    {
        return this.Add(new GaugeMetric(name));
    }

    /// <summary>
    /// Ends the current frame. The value of each metric becomes the value of this frame, and a new frame starts.
    /// </summary>
    /// <remarks>
    /// The application calls this once per frame. Calling it from other code breaks the values of the metrics.
    /// </remarks>
    public void EndFrame()
    {
        foreach (var metric in this._all)
        {
            metric.EndFrame();
        }
    }

    private T Add<T>(T metric) where T : Metric
    {
        this._all.Add(metric);
        return metric;
    }
}
