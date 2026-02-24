namespace Imago.Assets.Animations;

/// <summary>
/// Represents the interpolation mode of an animation.
/// </summary>
public enum InterpolationMode
{
    /// <summary>
    /// Step interpolation, where values change instantly at keyframes.
    /// </summary>
    Step,

    /// <summary>
    /// Linear interpolation between keyframes.
    /// </summary>
    Linear,

    /// <summary>
    /// Cubic spline interpolation for smooth curves.
    /// </summary>
    CubicSpline
}
