namespace Imago.Support;

/// <summary>
/// Represents an object that is updated every frame.
/// </summary>
public interface IUpdatable
{
    /// <summary>
    /// Updates the object for the current frame.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last frame, in seconds.</param>
    public void Update(float deltaTime);
}
