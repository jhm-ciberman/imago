using Imago.Rendering.Sprites;

namespace Imago.SceneGraph;

/// <summary>
/// Defines a contract for 2D layers that can be rendered.
/// </summary>
public interface ILayer2D : ILayer
{
    /// <summary>
    /// Gets the render target this layer should be drawn to.
    /// </summary>
    public LayerRenderTarget RenderTarget => LayerRenderTarget.Gui;

    /// <summary>
    /// Gets a value indicating whether the cursor is currently over any element in this layer.
    /// </summary>
    public bool IsCursorOverElement { get; }

    /// <summary>
    /// Draws the 2D layer using the specified drawing context.
    /// </summary>
    /// <param name="ctx">The drawing context to use for rendering.</param>
    public void Draw(DrawingContext ctx);
}
