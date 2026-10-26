namespace Imago.Support.Metrics;

/// <summary>
/// A metric that counts how many times something happens in each frame.
/// </summary>
public sealed class CounterMetric : Metric
{
    private int _frameCount;

    internal CounterMetric(string name)
        : base(name)
    {
    }

    /// <summary>
    /// Adds the given amount to the count of the current frame.
    /// </summary>
    /// <param name="count">The amount to add.</param>
    public void Add(int count = 1)
    {
        this._frameCount += count;
    }

    private protected override float TakeFrameValue()
    {
        int value = this._frameCount;
        this._frameCount = 0;
        return value;
    }
}
