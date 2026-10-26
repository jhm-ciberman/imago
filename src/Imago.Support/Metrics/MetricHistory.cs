namespace Imago.Support.Metrics;

/// <summary>
/// Tracks a rolling history of samples and computes aggregate statistics.
/// </summary>
public class MetricHistory
{
    private readonly float[] _samples;
    private int _index;
    private bool _filled;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricHistory"/> class.
    /// </summary>
    /// <param name="capacity">The number of samples to retain.</param>
    public MetricHistory(int capacity)
    {
        this._samples = new float[capacity];
    }

    /// <summary>
    /// Gets the sample buffer. Use with <see cref="Count"/> and <see cref="Offset"/>
    /// to read the ring buffer in chronological order.
    /// </summary>
    public float[] Samples => this._samples;

    /// <summary>
    /// Gets the number of valid samples currently stored.
    /// </summary>
    public int Count => this._filled ? this._samples.Length : this._index;

    /// <summary>
    /// Gets the offset into <see cref="Samples"/> where the oldest entry resides.
    /// </summary>
    public int Offset => this._filled ? this._index : 0;

    /// <summary>
    /// Gets the average value across all stored samples.
    /// </summary>
    public float Average { get; private set; }

    /// <summary>
    /// Gets the minimum value across all stored samples.
    /// </summary>
    public float Min { get; private set; }

    /// <summary>
    /// Gets the maximum value across all stored samples.
    /// </summary>
    public float Max { get; private set; }

    /// <summary>
    /// Records a new sample and recomputes the aggregate statistics.
    /// </summary>
    /// <param name="value">The value to record.</param>
    public void Record(float value)
    {
        this._samples[this._index] = value;
        this._index = (this._index + 1) % this._samples.Length;
        if (this._index == 0)
        {
            this._filled = true;
        }

        this.Recompute();
    }

    private void Recompute()
    {
        int count = this.Count;
        if (count == 0)
        {
            this.Average = 0;
            this.Min = 0;
            this.Max = 0;
            return;
        }

        float sum = 0;
        float min = float.MaxValue;
        float max = float.MinValue;

        for (int i = 0; i < count; i++)
        {
            float v = this._samples[i];
            sum += v;
            if (v < min) min = v;
            if (v > max) max = v;
        }

        this.Average = sum / count;
        this.Min = min;
        this.Max = max;
    }
}
