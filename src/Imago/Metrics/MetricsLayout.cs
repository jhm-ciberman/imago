using System;
using System.Collections.Generic;
using Imago.Support.Metrics;

namespace Imago.Metrics;

internal interface IMetricsLayoutNode<T>
{
    public string Label { get; }

    public List<T> ChildList { get; }
}

/// <summary>
/// Describes how the metrics of an application are shown: a tree of times and a list of count lines.
/// </summary>
/// <remarks>
/// <para>
/// Each entry has a path of labels separated by <c>/</c>, like <c>"Render/Main layers/3D"</c>. Missing parents are
/// created without a value. A new entry goes after its siblings, or before the sibling named by <c>before</c>.
/// </para>
/// <para>
/// The top-level times are the phases of a frame. They must not overlap, because the time left in the frame after
/// all of them is shown as waiting time.
/// </para>
/// </remarks>
public sealed class MetricsLayout
{
    private readonly List<MetricsTimeRow> _timeRows = [];
    private readonly List<MetricsLine> _lines = [];

    internal MetricsLayout()
    {
        //
    }

    /// <summary>
    /// Gets the top-level rows of the time tree.
    /// </summary>
    public IReadOnlyList<MetricsTimeRow> TimeRows => this._timeRows;

    /// <summary>
    /// Gets the count lines.
    /// </summary>
    public IReadOnlyList<MetricsLine> Lines => this._lines;

    /// <summary>
    /// Adds a timer to the time tree.
    /// </summary>
    /// <param name="path">The path of the row.</param>
    /// <param name="timer">The timer shown by the row.</param>
    /// <param name="before">The label of the sibling to put the row before, or null to put it last.</param>
    /// <returns>This layout.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the row already has timers.</exception>
    /// <exception cref="ArgumentException">Thrown if no sibling has the label in <paramref name="before"/>.</exception>
    public MetricsLayout Time(string path, TimerMetric timer, string? before = null)
    {
        return this.Time(path, [timer], before);
    }

    /// <summary>
    /// Adds timers to the time tree. The row shows the sum of their times.
    /// </summary>
    /// <param name="path">The path of the row.</param>
    /// <param name="timers">The timers shown by the row.</param>
    /// <param name="before">The label of the sibling to put the row before, or null to put it last.</param>
    /// <returns>This layout.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the row already has timers.</exception>
    /// <exception cref="ArgumentException">Thrown if no sibling has the label in <paramref name="before"/>.</exception>
    public MetricsLayout Time(string path, IReadOnlyList<TimerMetric> timers, string? before = null)
    {
        var row = GetOrAdd(this._timeRows, path, label => new MetricsTimeRow(label), before);
        if (row.TimerList.Count > 0)
        {
            throw new InvalidOperationException($"The time \"{path}\" already has timers.");
        }

        row.TimerList.AddRange(timers);
        return this;
    }

    /// <summary>
    /// Adds a count line, or a small value on a line when the path has two labels.
    /// </summary>
    /// <param name="path">The path of the line.</param>
    /// <param name="metric">The metric shown as a whole number.</param>
    /// <param name="before">The label of the sibling to put the line before, or null to put it last.</param>
    /// <returns>This layout.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the line already has a value.</exception>
    /// <exception cref="ArgumentException">Thrown if the path has more than two labels, or if no sibling has the label in <paramref name="before"/>.</exception>
    public MetricsLayout Count(string path, Metric metric, string? before = null)
    {
        return this.AddLine(path, MetricsLineFormat.Count, metric, null, before);
    }

    /// <summary>
    /// Adds a line that shows a part of a whole, like "74 of 235".
    /// </summary>
    /// <param name="path">The path of the line.</param>
    /// <param name="part">The metric for the part.</param>
    /// <param name="whole">The metric for the whole.</param>
    /// <param name="before">The label of the sibling to put the line before, or null to put it last.</param>
    /// <returns>This layout.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the line already has a value.</exception>
    /// <exception cref="ArgumentException">Thrown if the path has more than two labels, or if no sibling has the label in <paramref name="before"/>.</exception>
    public MetricsLayout Fraction(string path, Metric part, Metric whole, string? before = null)
    {
        return this.AddLine(path, MetricsLineFormat.Fraction, part, whole, before);
    }

    /// <summary>
    /// Adds a line that shows how many times something happens per second and per minute.
    /// </summary>
    /// <param name="path">The path of the line.</param>
    /// <param name="counter">The counter of events.</param>
    /// <param name="before">The label of the sibling to put the line before, or null to put it last.</param>
    /// <returns>This layout.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the line already has a value.</exception>
    /// <exception cref="ArgumentException">Thrown if the path has more than two labels, or if no sibling has the label in <paramref name="before"/>.</exception>
    public MetricsLayout Rate(string path, CounterMetric counter, string? before = null)
    {
        return this.AddLine(path, MetricsLineFormat.Rate, counter, null, before);
    }

    /// <summary>
    /// Adds a line that shows a number of bytes per frame and per second.
    /// </summary>
    /// <param name="path">The path of the line.</param>
    /// <param name="bytes">The counter of bytes.</param>
    /// <param name="before">The label of the sibling to put the line before, or null to put it last.</param>
    /// <returns>This layout.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the line already has a value.</exception>
    /// <exception cref="ArgumentException">Thrown if the path has more than two labels, or if no sibling has the label in <paramref name="before"/>.</exception>
    public MetricsLayout Bytes(string path, CounterMetric bytes, string? before = null)
    {
        return this.AddLine(path, MetricsLineFormat.Bytes, bytes, null, before);
    }

    private MetricsLayout AddLine(string path, MetricsLineFormat format, Metric metric, Metric? whole, string? before)
    {
        if (path.Split('/').Length > 2)
        {
            throw new ArgumentException($"The line \"{path}\" has more than two labels.", nameof(path));
        }

        var line = GetOrAdd(this._lines, path, label => new MetricsLine(label), before);
        if (line.Metric != null)
        {
            throw new InvalidOperationException($"The line \"{path}\" already has a value.");
        }

        line.Format = format;
        line.Metric = metric;
        line.Whole = whole;
        return this;
    }

    private static T GetOrAdd<T>(List<T> roots, string path, Func<string, T> create, string? before)
        where T : class, IMetricsLayoutNode<T>
    {
        string[] labels = path.Split('/');
        var siblings = roots;
        T? node = null;

        for (int i = 0; i < labels.Length; i++)
        {
            string label = labels[i];
            node = siblings.Find(n => n.Label == label);
            if (node == null)
            {
                node = create(label);
                bool isLast = i == labels.Length - 1;
                Insert(siblings, node, isLast ? before : null);
            }

            siblings = node.ChildList;
        }

        return node!;
    }

    private static void Insert<T>(List<T> siblings, T node, string? before)
        where T : class, IMetricsLayoutNode<T>
    {
        if (before == null)
        {
            siblings.Add(node);
            return;
        }

        int index = siblings.FindIndex(n => n.Label == before);
        if (index < 0)
        {
            throw new ArgumentException($"There is no \"{before}\" to put \"{node.Label}\" before.", nameof(before));
        }

        siblings.Insert(index, node);
    }
}
