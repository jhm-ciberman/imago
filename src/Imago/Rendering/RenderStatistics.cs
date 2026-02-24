using System.Diagnostics;

namespace Imago.Rendering;

/// <summary>
/// Tracks a rolling history of frame time samples and computes aggregate statistics.
/// </summary>
public class TimeHistory
{
    private readonly float[] _samples;
    private int _index;
    private bool _filled;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeHistory"/> class.
    /// </summary>
    /// <param name="capacity">The number of samples to retain.</param>
    public TimeHistory(int capacity)
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
    /// <param name="value">The time value in milliseconds.</param>
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

/// <summary>
/// Measures the elapsed time of a rendering phase using double-buffered snapshots.
/// Call <see cref="Begin"/> and <see cref="End"/> to bracket the work, then read
/// <see cref="TimeMs"/> and <see cref="History"/> for the previous frame's result.
/// </summary>
/// <remarks>
/// Multiple <see cref="Begin"/>/<see cref="End"/> pairs within a single frame are accumulated.
/// </remarks>
public class PhaseTimer
{
    private readonly Stopwatch _stopwatch = new();
    private float _currentTimeMs;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhaseTimer"/> class.
    /// </summary>
    /// <param name="historyCapacity">The number of samples to retain in the rolling history.</param>
    public PhaseTimer(int historyCapacity)
    {
        this.History = new TimeHistory(historyCapacity);
    }

    /// <summary>
    /// Gets the elapsed time in milliseconds from the previous frame.
    /// </summary>
    public float TimeMs { get; private set; }

    /// <summary>
    /// Gets the rolling history of elapsed times in milliseconds.
    /// </summary>
    public TimeHistory History { get; }

    /// <summary>
    /// Starts measuring this phase.
    /// </summary>
    public void Begin()
    {
        this._stopwatch.Restart();
    }

    /// <summary>
    /// Stops measuring this phase and accumulates the elapsed time.
    /// </summary>
    public void End()
    {
        this._stopwatch.Stop();
        this._currentTimeMs += (float)this._stopwatch.Elapsed.TotalMilliseconds;
    }

    /// <summary>
    /// Snapshots the accumulated time into <see cref="TimeMs"/>, records it
    /// into <see cref="History"/>, and resets the accumulator for the next frame.
    /// </summary>
    internal void BeginFrame()
    {
        this.TimeMs = this._currentTimeMs;
        this.History.Record(this.TimeMs);
        this._currentTimeMs = 0;
    }
}

/// <summary>
/// Holds per-frame rendering statistics using double-buffering. Public properties expose
/// the previous frame's completed values, while internal fields accumulate the current frame's data.
/// </summary>
/// <remarks>
/// Call <see cref="BeginFrame"/> at the start of each render pass to snapshot the previous frame's
/// stats and reset the accumulators.
/// </remarks>
public class RenderStatistics
{
    private const int HistoryCapacity = 300;

    internal int CurrentDrawCalls;
    internal int CurrentShadowDrawCalls;
    internal int CurrentVisibleRenderables;
    internal int CurrentTotalRenderables;
    internal int CurrentGuiDrawCalls;

    /// <summary>
    /// Gets the number of draw calls issued by the forward rendering pass.
    /// </summary>
    public int DrawCalls { get; private set; }

    /// <summary>
    /// Gets the number of draw calls issued by the shadow mapping pass.
    /// </summary>
    public int ShadowDrawCalls { get; private set; }

    /// <summary>
    /// Gets the number of renderables visible after frustum culling.
    /// </summary>
    public int VisibleRenderables { get; private set; }

    /// <summary>
    /// Gets the total number of renderables before frustum culling.
    /// </summary>
    public int TotalRenderables { get; private set; }

    /// <summary>
    /// Gets the number of 2D sprite draw calls issued by the GUI.
    /// </summary>
    public int GuiDrawCalls { get; private set; }

    /// <summary>
    /// Gets the total render time, excluding VSync wait.
    /// </summary>
    public PhaseTimer RenderTime { get; } = new(HistoryCapacity);

    /// <summary>
    /// Gets the total update time (input, game logic, scene graph).
    /// </summary>
    public PhaseTimer UpdateTime { get; } = new(HistoryCapacity);

    /// <summary>
    /// Gets the scene preparation phase time (scene graph traversal, ImGui building, resource uploads).
    /// </summary>
    public PhaseTimer PreparePhase { get; } = new(HistoryCapacity);

    /// <summary>
    /// Gets the 3D rendering phase time (shadow pass + forward pass).
    /// </summary>
    public PhaseTimer Pass3D { get; } = new(HistoryCapacity);

    /// <summary>
    /// Gets the 2D rendering phase time (sprite layers, fullscreen compositing, ImGui).
    /// </summary>
    public PhaseTimer Pass2D { get; } = new(HistoryCapacity);

    /// <summary>
    /// Gets the GPU synchronization phase time (WaitForIdle + command submission).
    /// </summary>
    public PhaseTimer GpuSync { get; } = new(HistoryCapacity);

    /// <summary>
    /// Gets the shadow mapping sub-phase time within the 3D pass.
    /// </summary>
    public PhaseTimer ShadowSubPass { get; } = new(HistoryCapacity);

    /// <summary>
    /// Gets the forward rendering sub-phase time within the 3D pass.
    /// </summary>
    public PhaseTimer ForwardSubPass { get; } = new(HistoryCapacity);

    /// <summary>
    /// Gets the mouse picking sub-phase time within the 3D pass.
    /// </summary>
    public PhaseTimer PickingSubPass { get; } = new(HistoryCapacity);

    /// <summary>
    /// Gets the combined time of minor 3D sub-phases (sky dome, immediate, particles, gizmos).
    /// </summary>
    public PhaseTimer OtherSubPass { get; } = new(HistoryCapacity);

    /// <summary>
    /// Snapshots the current frame's statistics into the public properties
    /// and resets the accumulators for the next frame.
    /// </summary>
    public void BeginFrame()
    {
        this.DrawCalls = this.CurrentDrawCalls;
        this.ShadowDrawCalls = this.CurrentShadowDrawCalls;
        this.VisibleRenderables = this.CurrentVisibleRenderables;
        this.TotalRenderables = this.CurrentTotalRenderables;
        this.GuiDrawCalls = this.CurrentGuiDrawCalls;

        this.RenderTime.BeginFrame();
        this.UpdateTime.BeginFrame();
        this.PreparePhase.BeginFrame();
        this.Pass3D.BeginFrame();
        this.Pass2D.BeginFrame();
        this.GpuSync.BeginFrame();
        this.ShadowSubPass.BeginFrame();
        this.ForwardSubPass.BeginFrame();
        this.PickingSubPass.BeginFrame();
        this.OtherSubPass.BeginFrame();

        this.CurrentDrawCalls = 0;
        this.CurrentShadowDrawCalls = 0;
        this.CurrentVisibleRenderables = 0;
        this.CurrentTotalRenderables = 0;
        this.CurrentGuiDrawCalls = 0;
    }
}
