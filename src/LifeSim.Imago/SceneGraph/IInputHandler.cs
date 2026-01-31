using LifeSim.Imago.Input;

namespace LifeSim.Imago.SceneGraph;

/// <summary>
/// Defines a contract for layers that handle user input.
/// </summary>
/// <remarks>
/// Layers implementing this interface will receive input events routed by the Stage
/// in reverse ZOrder (highest first). Input stops propagating when an event is marked as handled.
/// </remarks>
public interface IInputHandler : ILayer
{
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
