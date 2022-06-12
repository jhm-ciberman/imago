namespace LifeSim.Engine.Controls;

/// <summary>
/// A brush that is animated.
/// </summary>
public interface IAnimatedBrush : IBrush
{
    void Update(float deltaTime);
}
