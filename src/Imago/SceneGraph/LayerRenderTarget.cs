namespace Imago.SceneGraph;

/// <summary>
/// Specifies which render target a layer should be drawn to.
/// </summary>
public enum LayerRenderTarget
{
    /// <summary>
    /// Rendered to the main render texture, below the GUI. The target has color and depth.
    /// </summary>
    Main,

    /// <summary>
    /// Rendered to the GUI render texture, which is upscaled with the pixel art shader.
    /// </summary>
    Gui,

    /// <summary>
    /// Rendered directly to the final target, on top of all other content.
    /// </summary>
    Overlay
}
