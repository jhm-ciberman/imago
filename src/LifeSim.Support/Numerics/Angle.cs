namespace LifeSim.Support.Numerics;

/// <summary>
/// Provides utility methods for working with angles in radians.
/// </summary>
public static class Angle
{
    /// <summary>
    /// Returns the difference between two angles in radians. This is the shortest
    /// distance between the two angles, taking into account the wrap-around at 2*Pi.
    /// The result is in the range [-Pi, Pi].
    /// </summary>
    /// <param name="current">The current angle in radians.</param>
    /// <param name="target">The target angle in radians.</param>
    /// <returns>The difference between the two angles in radians.</returns>
    public static float Difference(float current, float target)
    {
        float delta = (target - current) % (float.Pi * 2f);

        if (delta > float.Pi)
        {
            delta -= float.Pi * 2;
        }
        else if (delta < -float.Pi)
        {
            delta += float.Pi * 2;
        }

        return delta;
    }

    /// <summary>
    /// Returns the angle in radians between 0 and 2*Pi.
    /// </summary>
    /// <param name="value">The angle in radians.</param>
    /// <returns>The angle in the range [0, 2*Pi).</returns>
    public static float Wrap(float value)
    {
        float twoPi = float.Pi * 2f;
        float result = value % twoPi;
        if (result < 0)
        {
            result += twoPi;
        }
        return result;
    }

    /// <summary>
    /// Linearly interpolates the angle in radians.
    /// </summary>
    /// <param name="a">The first angle in radians.</param>
    /// <param name="b">The second angle in radians.</param>
    /// <param name="t">The interpolation factor.</param>
    /// <returns>The interpolated angle in radians.</returns>
    public static float Lerp(float a, float b, float t)
    {
        float delta = Difference(a, b);
        return a + delta * t;
    }

    /// <summary>
    /// Clamps an angle to be within a specified offset range from a reference angle.
    /// Useful for limiting camera/head rotation relative to body facing direction.
    /// </summary>
    /// <param name="angle">The angle to clamp in radians.</param>
    /// <param name="reference">The reference angle in radians (e.g., body facing direction).</param>
    /// <param name="minOffset">The minimum allowed offset from reference in radians (typically negative).</param>
    /// <param name="maxOffset">The maximum allowed offset from reference in radians (typically positive).</param>
    /// <returns>The clamped angle in radians.</returns>
    public static float ClampRelative(float angle, float reference, float minOffset, float maxOffset)
    {
        float offset = Difference(reference, angle);
        float clampedOffset = float.Clamp(offset, minOffset, maxOffset);
        return reference + clampedOffset;
    }

    /// <summary>
    /// Clamps an angle to be within a symmetric offset range from a reference angle.
    /// For example, to limit head rotation to ±90° from body facing direction.
    /// </summary>
    /// <param name="angle">The angle to clamp in radians.</param>
    /// <param name="reference">The reference angle in radians (e.g., body facing direction).</param>
    /// <param name="maxOffset">The maximum allowed offset from reference in radians (e.g., Pi/2 for 90 degrees).</param>
    /// <returns>The clamped angle in radians.</returns>
    public static float ClampRelative(float angle, float reference, float maxOffset)
    {
        return ClampRelative(angle, reference, -maxOffset, maxOffset);
    }
}
