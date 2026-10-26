using Imago.Support.Metrics;

namespace Imago.Metrics;

/// <summary>
/// The metrics of the application loop and the renderer.
/// </summary>
public sealed class ImagoMetrics : MetricGroup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImagoMetrics"/> class.
    /// </summary>
    public ImagoMetrics()
        : base("Imago")
    {
        this.FrameTime = this.Timer("Frame time");
        this.UpdateTime = this.Timer("Update time");
        this.GameUpdateTime = this.Timer("Game update time");
        this.StageUpdateTime = this.Timer("Stage update time");
        this.RenderTime = this.Timer("Render time");
        this.PrepareTime = this.Timer("Prepare time");
        this.MainLayersTime = this.Timer("Main layers time");
        this.GuiLayersTime = this.Timer("GUI layers time");
        this.GpuSyncTime = this.Timer("GPU sync time");
        this.SpriteDrawCallCount = this.Counter("Sprite draw call count");
        this.TextureUploadCount = this.Counter("Texture upload count");
        this.AllocatedBytes = this.Counter("Allocated bytes");
        this.Gen0CollectionCount = this.Counter("Gen 0 collection count");
        this.Gen1CollectionCount = this.Counter("Gen 1 collection count");
        this.Gen2CollectionCount = this.Counter("Gen 2 collection count");
    }

    /// <summary>
    /// Gets the time between the last two frames.
    /// </summary>
    public TimerMetric FrameTime { get; }

    /// <summary>
    /// Gets the time of the whole update: input, game, and stage.
    /// </summary>
    public TimerMetric UpdateTime { get; }

    /// <summary>
    /// Gets the time of the game update and post update.
    /// </summary>
    public TimerMetric GameUpdateTime { get; }

    /// <summary>
    /// Gets the time of the stage update.
    /// </summary>
    public TimerMetric StageUpdateTime { get; }

    /// <summary>
    /// Gets the time of the whole render, without waiting for the screen.
    /// </summary>
    public TimerMetric RenderTime { get; }

    /// <summary>
    /// Gets the time spent preparing the frame, like uploading changed textures.
    /// </summary>
    public TimerMetric PrepareTime { get; }

    /// <summary>
    /// Gets the time spent rendering the main layers.
    /// </summary>
    public TimerMetric MainLayersTime { get; }

    /// <summary>
    /// Gets the time spent rendering the GUI and overlay layers and putting the final image together.
    /// </summary>
    public TimerMetric GuiLayersTime { get; }

    /// <summary>
    /// Gets the time spent waiting for the GPU and sending the commands.
    /// </summary>
    public TimerMetric GpuSyncTime { get; }

    /// <summary>
    /// Gets the number of sprite draw calls.
    /// </summary>
    public CounterMetric SpriteDrawCallCount { get; }

    /// <summary>
    /// Gets the number of textures uploaded to the GPU.
    /// </summary>
    public CounterMetric TextureUploadCount { get; }

    /// <summary>
    /// Gets the number of bytes allocated on the main thread.
    /// </summary>
    public CounterMetric AllocatedBytes { get; }

    /// <summary>
    /// Gets the number of garbage collections that included generation 0. Every collection does.
    /// </summary>
    public CounterMetric Gen0CollectionCount { get; }

    /// <summary>
    /// Gets the number of garbage collections that included generation 1.
    /// </summary>
    public CounterMetric Gen1CollectionCount { get; }

    /// <summary>
    /// Gets the number of garbage collections that included generation 2, the full collections.
    /// </summary>
    public CounterMetric Gen2CollectionCount { get; }
}
