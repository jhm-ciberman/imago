namespace Imago;

/// <summary>
/// Marks a render layer that can draw into any render texture, not only the screen.
/// </summary>
/// <remarks>
/// Only these layers can be rendered with <see cref="Rendering.Renderer.RenderLayerOffscreen"/>
/// or shown inside a GUI control.
/// </remarks>
public interface IOffscreenLayer : IRenderLayer
{
}
