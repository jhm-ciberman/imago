namespace Imago.SceneGraph;

/// <summary>
/// Runtime settings for the scene's screen-space ambient occlusion effect.
/// </summary>
public class AmbientOcclusionSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether ambient occlusion is applied to the scene.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets how strongly occlusion darkens the scene.
    /// </summary>
    public float Intensity { get; set; } = 0.37f;

    /// <summary>
    /// Gets or sets the sampling radius in world units. Larger values capture broader occlusion.
    /// </summary>
    public float Radius { get; set; } = 0.4f;

    /// <summary>
    /// Gets or sets the exponent applied to the occlusion factor to shape its falloff.
    /// </summary>
    public float Power { get; set; } = 1.133f;

    /// <summary>
    /// Gets or sets the depth bias used to avoid self-occlusion artifacts.
    /// </summary>
    public float Bias { get; set; } = 0.043f;
}
