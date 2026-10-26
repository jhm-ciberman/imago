namespace Imago.Support.Metrics;

/// <summary>
/// A metric that holds a value at the end of each frame, like the number of items waiting in a queue.
/// </summary>
/// <remarks>
/// The value stays the same in the next frames until it is set again.
/// </remarks>
public sealed class GaugeMetric : Metric
{
    private float _current;

    internal GaugeMetric(string name)
        : base(name)
    {
    }

    /// <summary>
    /// Sets the current value.
    /// </summary>
    /// <param name="value">The new value.</param>
    public void Set(float value)
    {
        this._current = value;
    }

    private protected override float TakeFrameValue()
    {
        return this._current;
    }
}
