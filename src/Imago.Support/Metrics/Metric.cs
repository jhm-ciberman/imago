namespace Imago.Support.Metrics;

/// <summary>
/// A number that is recorded every frame, like the time of a render pass or the number of draw calls.
/// </summary>
public abstract class Metric
{
    private const int HistoryCapacity = 300;

    private protected Metric(string name)
    {
        this.Name = name;
    }

    /// <summary>
    /// Gets the name of the metric.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the value of the last finished frame.
    /// </summary>
    public float Value { get; private set; }

    /// <summary>
    /// Gets the values of the last finished frames.
    /// </summary>
    public MetricHistory History { get; } = new(HistoryCapacity);

    internal void EndFrame()
    {
        this.Value = this.TakeFrameValue();
        this.History.Record(this.Value);
    }

    private protected abstract float TakeFrameValue();
}
