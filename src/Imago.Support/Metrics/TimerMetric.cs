using System;
using System.Diagnostics;

namespace Imago.Support.Metrics;

/// <summary>
/// Measures the time from <see cref="TimerMetric.StartTimer"/> until it is disposed.
/// </summary>
public readonly ref struct TimerMetricScope
{
    private readonly TimerMetric _metric;
    private readonly long _startTimestamp;

    internal TimerMetricScope(TimerMetric metric)
    {
        this._metric = metric;
        this._startTimestamp = Stopwatch.GetTimestamp();
    }

    /// <summary>
    /// Stops measuring and adds the time to the metric.
    /// </summary>
    public void Dispose()
    {
        this._metric.Add(Stopwatch.GetElapsedTime(this._startTimestamp));
    }
}

/// <summary>
/// A metric that measures the time spent in each frame, in milliseconds.
/// </summary>
/// <remarks>
/// When the time is measured more than once in a frame, the times are added together.
/// </remarks>
public sealed class TimerMetric : Metric
{
    private double _frameMilliseconds;

    internal TimerMetric(string name)
        : base(name)
    {
    }

    /// <summary>
    /// Starts measuring. The time is added when the returned scope is disposed.
    /// </summary>
    /// <returns>The scope to dispose when the work ends.</returns>
    public TimerMetricScope StartTimer()
    {
        return new TimerMetricScope(this);
    }

    /// <summary>
    /// Adds a time that was measured in another way to the current frame.
    /// </summary>
    /// <param name="elapsed">The time to add.</param>
    public void Add(TimeSpan elapsed)
    {
        this._frameMilliseconds += elapsed.TotalMilliseconds;
    }

    private protected override float TakeFrameValue()
    {
        float value = (float)this._frameMilliseconds;
        this._frameMilliseconds = 0;
        return value;
    }
}
