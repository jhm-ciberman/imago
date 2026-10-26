using System.Collections.Generic;
using Imago.Support.Metrics;

namespace Imago.Metrics;

/// <summary>
/// Specifies how the value of a <see cref="MetricsLine"/> is shown.
/// </summary>
public enum MetricsLineFormat
{
    /// <summary>A whole number.</summary>
    Count,

    /// <summary>A part of a whole, like "74 of 235".</summary>
    Fraction,

    /// <summary>A number of bytes, shown per frame and per second.</summary>
    Bytes,

    /// <summary>A number of events, shown per second and per minute.</summary>
    Rate,
}

/// <summary>
/// A line in the count section of a <see cref="MetricsLayout"/>, with optional small values on the same line.
/// </summary>
public sealed class MetricsLine : IMetricsLayoutNode<MetricsLine>
{
    private readonly List<MetricsLine> _children = [];

    internal MetricsLine(string label)
    {
        this.Label = label;
    }

    /// <summary>
    /// Gets the text shown for the line.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets how the value is shown.
    /// </summary>
    public MetricsLineFormat Format { get; internal set; }

    /// <summary>
    /// Gets the metric shown as the value of the line, or null if the line only shows its children.
    /// </summary>
    public Metric? Metric { get; internal set; }

    /// <summary>
    /// Gets the metric for the whole when the line shows a fraction, or null otherwise.
    /// </summary>
    public Metric? Whole { get; internal set; }

    /// <summary>
    /// Gets the small values shown on the same line, after the value.
    /// </summary>
    public IReadOnlyList<MetricsLine> Children => this._children;

    List<MetricsLine> IMetricsLayoutNode<MetricsLine>.ChildList => this._children;
}
