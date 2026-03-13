namespace Imago.SceneGraph;

/// <summary>
/// Specifies which render target a 2D layer should be drawn to.
/// </summary>
public enum LayerRenderTarget
{
    /// <summary>
    /// Rendered to the GUI render texture, which is upscaled with the pixel art shader.
    /// </summary>
    Gui,

    /// <summary>
    /// Rendered directly to the swapchain after ImGui, on top of all other content.
    /// </summary>
    Overlay
}
