using Imago.Support.Metrics;

namespace Imago.Graphics3D.Metrics;

/// <summary>
/// The metrics of the 3D renderer.
/// </summary>
public sealed class Graphics3DMetrics : MetricGroup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Graphics3DMetrics"/> class.
    /// </summary>
    public Graphics3DMetrics()
        : base("Graphics3D")
    {
        this.SceneUpdateTime = this.Timer("Scene update time");
        this.SceneRenderTime = this.Timer("Scene render time");
        this.BufferUploadTime = this.Timer("Buffer upload time");
        this.ShadowPassTime = this.Timer("Shadow pass time");
        this.ForwardPassTime = this.Timer("Forward pass time");
        this.SkyDomePassTime = this.Timer("Sky dome pass time");
        this.AmbientOcclusionPassTime = this.Timer("Ambient occlusion pass time");
        this.ImmediatePassTime = this.Timer("Immediate pass time");
        this.ParticlesPassTime = this.Timer("Particles pass time");
        this.PickingPassTime = this.Timer("Picking pass time");
        this.GizmosPassTime = this.Timer("Gizmos pass time");
        this.DrawCallCount = this.Counter("Draw call count");
        this.ShadowDrawCallCount = this.Counter("Shadow draw call count");
        this.PickingDrawCallCount = this.Counter("Picking draw call count");
        this.VisibleRenderableCount = this.Counter("Visible renderable count");
        this.TotalRenderableCount = this.Counter("Total renderable count");
        this.ParticleCount = this.Counter("Particle count");
        this.ShadowCascadeCount = this.Gauge("Shadow cascade count");
    }

    /// <summary>
    /// Gets the time of the update of the 3D scenes, like reading the object under the cursor.
    /// </summary>
    public TimerMetric SceneUpdateTime { get; }

    /// <summary>
    /// Gets the time of the whole render of the 3D scenes.
    /// </summary>
    public TimerMetric SceneRenderTime { get; }

    /// <summary>
    /// Gets the time spent uploading instance, transform, and skeleton data to the GPU.
    /// </summary>
    public TimerMetric BufferUploadTime { get; }

    /// <summary>
    /// Gets the time of the shadow pass.
    /// </summary>
    public TimerMetric ShadowPassTime { get; }

    /// <summary>
    /// Gets the time of the forward pass, for opaque and transparent objects.
    /// </summary>
    public TimerMetric ForwardPassTime { get; }

    /// <summary>
    /// Gets the time of the sky dome pass.
    /// </summary>
    public TimerMetric SkyDomePassTime { get; }

    /// <summary>
    /// Gets the time of the ambient occlusion pass.
    /// </summary>
    public TimerMetric AmbientOcclusionPassTime { get; }

    /// <summary>
    /// Gets the time of the immediate pass.
    /// </summary>
    public TimerMetric ImmediatePassTime { get; }

    /// <summary>
    /// Gets the time of the particles pass.
    /// </summary>
    public TimerMetric ParticlesPassTime { get; }

    /// <summary>
    /// Gets the time of the mouse picking pass.
    /// </summary>
    public TimerMetric PickingPassTime { get; }

    /// <summary>
    /// Gets the time of the gizmos pass.
    /// </summary>
    public TimerMetric GizmosPassTime { get; }

    /// <summary>
    /// Gets the number of draw calls of the forward pass.
    /// </summary>
    public CounterMetric DrawCallCount { get; }

    /// <summary>
    /// Gets the number of draw calls of the shadow pass.
    /// </summary>
    public CounterMetric ShadowDrawCallCount { get; }

    /// <summary>
    /// Gets the number of draw calls of the mouse picking pass.
    /// </summary>
    public CounterMetric PickingDrawCallCount { get; }

    /// <summary>
    /// Gets the number of renderables that passed frustum culling.
    /// </summary>
    public CounterMetric VisibleRenderableCount { get; }

    /// <summary>
    /// Gets the number of renderables before frustum culling.
    /// </summary>
    public CounterMetric TotalRenderableCount { get; }

    /// <summary>
    /// Gets the number of particles drawn.
    /// </summary>
    public CounterMetric ParticleCount { get; }

    /// <summary>
    /// Gets the number of shadow cascades of the last rendered scene.
    /// </summary>
    public GaugeMetric ShadowCascadeCount { get; }
}
