using Support;

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
}
