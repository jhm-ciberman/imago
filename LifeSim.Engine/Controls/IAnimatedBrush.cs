namespace LifeSim.Engine.Controls;

/// <summary>
/// A brush that is animated.
/// </summary>
public interface IAnimatedBrush : IBrush
{
    /// <summary>
    /// Updates the brush.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    void Update(float deltaTime);
}
