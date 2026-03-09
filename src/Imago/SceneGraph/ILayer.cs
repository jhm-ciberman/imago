namespace Imago.SceneGraph;

/// <summary>
/// Base interface for anything that can be composed into the render pipeline.
/// </summary>
public interface ILayer : IMountable
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
    /// Gets the stage this layer belongs to, or <c>null</c> if unmounted.
    /// </summary>
    public Stage? Stage { get; }

    /// <summary>
    /// Gets or sets a value indicating whether input to this layer is blocked by a higher layer.
    /// </summary>
    /// <remarks>
    /// Computed by <see cref="Stage"/> each frame. When true, this layer should not process
    /// input events or hit testing because a higher layer with input blocking is active.
    /// </remarks>
    public bool IsInputBlocked { get; set; }

    /// <summary>
    /// Mounts this layer to the given <see cref="Stage"/>.
    /// </summary>
    /// <param name="stage">The stage to mount to.</param>
    public void Mount(Stage stage);

    /// <summary>
    /// Unmounts this layer from its <see cref="Stage"/>.
    /// </summary>
    public void Unmount();

    /// <summary>
    /// Updates the layer state.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in seconds.</param>
    public void Update(float deltaTime);
}
