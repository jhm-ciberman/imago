namespace LifeSim.Imago.SceneGraph;

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
    /// Updates the layer state.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in seconds.</param>
    public void Update(float deltaTime);
}
