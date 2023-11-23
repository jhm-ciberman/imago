using Imago.SceneGraph.Lighting;
using Support.Drawing;

namespace Imago.SceneGraph;

public class SceneEnvironment
{
    /// <summary>
    /// Gets or sets the main light of the scene.
    /// </summary>
    public DirectionalLight MainLight { get; set; } = new DirectionalLight();

    /// <summary>
    /// Gets or sets the ambient color of the scene.
    /// </summary>
    public ColorF AmbientColor { get; set; } = new ColorF(.2f, .2f, .2f, 100f / 255f);

    /// <summary>
    /// Gets or sets the fog color.
    /// </summary>
    public ColorF FogColor { get; set; } = new ColorF("#6d6b4e");

    /// <summary>
    /// Gets or sets the start distance of the fog.
    /// </summary>
    public float FogStart { get; set; } = 50f;

    /// <summary>
    /// Gets or sets the end distance of the fog.
    /// </summary>
    public float FogEnd { get; set; } = 300f;

    /// <summary>
    /// Gets or sets a value indicating whether the sky dome is enabled.
    /// </summary>
    public bool SkyDomeEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the progress of the day as a value between 0 and 1.
    /// </summary>
    public float SkyDomeDayProgress { get; set; } = 0.38f;
}
