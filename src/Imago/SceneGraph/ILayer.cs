namespace Imago.SceneGraph;

/// <summary>
/// Base interface for anything that can be composed into the render pipeline.
/// </summary>
public interface ILayer
{
    /// <summary>
    /// Gets the Z-order of this layer. Lower values render first (behind higher values).
    /// </summary>
    public int ZOrder { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this layer is visible and should be rendered.
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// Gets or sets the stage this layer belongs to.
    /// </summary>
    public Stage? Stage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether input to this layer is blocked by a higher layer.
    /// </summary>
    /// <remarks>
    /// Computed by <see cref="Stage"/> each frame. When true, this layer should not process
    /// input events or hit testing because a higher layer with input blocking is active.
    /// </remarks>
    public bool IsInputBlocked { get; set; }

    /// <summary>
    /// Updates the layer state.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in seconds.</param>
    public void Update(float deltaTime);
}
