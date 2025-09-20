using LifeSim.Imago.Input;
using LifeSim.Imago.Rendering.Sprites;

namespace LifeSim.Imago.SceneGraph;

/// <summary>
/// Defines a contract for 2D layers that can be rendered and handle user input.
/// </summary>
public interface ILayer2D
{
    /// <summary>
    /// Gets a value indicating whether the cursor is currently over any element in this layer.
    /// </summary>
    public bool IsCursorOverElement { get; }

    /// <summary>
    /// Draws the 2D layer using the specified drawing context.
    /// </summary>
    /// <param name="ctx">The drawing context to use for rendering.</param>
    public void Draw(DrawingContext ctx);

    /// <summary>
    /// Updates the layer with the specified time delta.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update in seconds.</param>
    public void Update(float deltaTime);

    /// <summary>
    /// Handles mouse button press events.
    /// </summary>
    /// <param name="e">The mouse button event arguments.</param>
    public void HandleMousePressed(MouseButtonEventArgs e);

    /// <summary>
    /// Handles mouse button release events.
    /// </summary>
    /// <param name="e">The mouse button event arguments.</param>
    public void HandleMouseReleased(MouseButtonEventArgs e);

    /// <summary>
    /// Handles mouse wheel scroll events.
    /// </summary>
    /// <param name="e">The mouse wheel event arguments.</param>
    public void HandleMouseWheel(MouseWheelEventArgs e);

    /// <summary>
    /// Handles keyboard key press events.
    /// </summary>
    /// <param name="e">The keyboard event arguments.</param>
    public void HandleKeyPressed(KeyboardEventArgs e);

    /// <summary>
    /// Handles keyboard key release events.
    /// </summary>
    /// <param name="e">The keyboard event arguments.</param>
    public void HandleKeyReleased(KeyboardEventArgs e);
}
