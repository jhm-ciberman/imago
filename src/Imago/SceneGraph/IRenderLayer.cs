namespace Imago.SceneGraph;

/// <summary>
/// Defines a contract for layers that render with direct access to the GPU.
/// </summary>
public interface IRenderLayer : ILayer
{
    /// <summary>
    /// Gets the render target this layer should be drawn to.
    /// </summary>
    public LayerRenderTarget RenderTarget => LayerRenderTarget.Gui;

    /// <summary>
    /// Records the draw commands of this layer.
    /// </summary>
    /// <param name="ctx">The GPU objects for the current frame.</param>
    public void Render(RenderContext ctx);
}
