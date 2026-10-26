using System.Collections.Generic;
using Imago.Support.Metrics;

namespace Imago.Metrics;

/// <summary>
/// A row in the time tree of a <see cref="MetricsLayout"/>.
/// </summary>
public sealed class MetricsTimeRow : IMetricsLayoutNode<MetricsTimeRow>
{
    private readonly List<TimerMetric> _timers = [];
    private readonly List<MetricsTimeRow> _children = [];

    internal MetricsTimeRow(string label)
    {
        this.Label = label;
    }

    /// <summary>
    /// Gets the text shown for the row.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets the timers whose times are added together to make the value of the row.
    /// </summary>
    /// <remarks>
    /// A row without timers takes the sum of its children as its value.
    /// </remarks>
    public IReadOnlyList<TimerMetric> Timers => this._timers;

    /// <summary>
    /// Gets the rows below this one.
    /// </summary>
    public IReadOnlyList<MetricsTimeRow> Children => this._children;

    internal List<TimerMetric> TimerList => this._timers;

    List<MetricsTimeRow> IMetricsLayoutNode<MetricsTimeRow>.ChildList => this._children;

    /// <summary>
    /// Gets the value of the row in the last finished frame, in milliseconds.
    /// </summary>
    public float Value
    {
        get
        {
            float sum = 0f;
            if (this._timers.Count > 0)
            {
                foreach (var timer in this._timers)
                {
                    sum += timer.Value;
                }
            }
            else
            {
                foreach (var child in this._children)
                {
                    sum += child.Value;
                }
            }

            return sum;
        }
    }

    /// <summary>
    /// Gets the value of the row in one of the frames kept in the history of its metrics.
    /// </summary>
    /// <param name="index">The index in <see cref="MetricHistory.Samples"/>. It is the same frame for every metric.</param>
    /// <returns>The value of the row in that frame, in milliseconds.</returns>
    public float GetSample(int index)
    {
        float sum = 0f;
        if (this._timers.Count > 0)
        {
            foreach (var timer in this._timers)
            {
                sum += timer.History.Samples[index];
            }
        }
        else
        {
            foreach (var child in this._children)
            {
                sum += child.GetSample(index);
            }
        }

        return sum;
    }
}
