using System;
using System.Numerics;

namespace LifeSim.Support.Numerics;

public static class MathUtils
{
    private const float K_EPSILON_NORMAL_SQRT = 1e-15f;

    /// <summary>
    /// Gets the value of pi as a float.
    /// </summary>
    public static float Pi { get; } = MathF.PI;

    /// <summary>
    /// Gets the value of pi divided by two.
    /// </summary>
    public static float HalfPi { get; } = MathF.PI / 2f;

    /// <summary>
    /// Gets the value of pi divided by four.
    /// </summary>
    public static float QuarterPi { get; } = MathF.PI / 4f;

    /// <summary>
    /// Gets the value of two times pi.
    /// </summary>
    public static float TwoPi { get; } = MathF.PI * 2f;

    /// <summary>
    /// Gets the value of pi divided by 180.
    /// </summary>
    public static float DegToRad { get; } = MathF.PI / 180f;

    /// <summary>
    /// Gets the value of 180 divided by pi.
    /// </summary>
    public static float RadToDeg { get; } = 180f / MathF.PI;

    /// <summary>
    /// Smoothly interpolates between two values.
    /// </summary>
    /// <param name="v1">The first value.</param>
    /// <param name="v2">The second value.</param>
    /// <param name="lerpProgress">The interpolation factor.</param>
    /// <returns>The interpolated value.</returns>
    public static float SmoothStep(float v1, float v2, float lerpProgress)
    {
        return float.Lerp(v1, v2, lerpProgress * lerpProgress * (3f - 2f * lerpProgress));
    }

    // Moves a value /current/ towards /target/.
    public static float MoveTowards(float current, float target, float maxDelta)
    {
        if (MathF.Abs(target - current) <= maxDelta)
            return target;
        return current + MathF.Sign(target - current) * maxDelta;
    }

    public static float AngleDifference(float current, float target)
    {
        float delta = (target - current) % (MathF.PI * 2);

        if (delta > MathF.PI)
            delta -= MathF.PI * 2;
        else if (delta < -MathF.PI)
            delta += MathF.PI * 2;

        return delta;
    }

    public static Vector2 ClampMagnitude(Vector2 velocity, float maxMagnitude)
    {
        if (velocity.LengthSquared() > maxMagnitude * maxMagnitude)
            return Vector2.Normalize(velocity) * maxMagnitude;
        return velocity;
    }

    /// <summary>
    /// Clamps a value between a minimum and maximum value.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>The clamped value.</returns>
    public static float Clamp(float value, float min, float max)
    {
        if (value < min)
            return min;
        if (value > max)
            return max;
        return value;
    }



    /// <summary>
    /// Returns the angle in radians between 0 and 2*Pi.
    /// </summary>
    /// <param name="value">The angle in radians.</param>
    /// <returns>The angle in the range [0, 2*Pi).</returns>
    public static float WrapAngle(float value)
    {
        return (value + TwoPi) % TwoPi;
    }

    /// <summary>
    /// Returns the angle in radians between -Pi and Pi.
    /// </summary>
    /// <param name="angle">The angle in radians.</param>
    /// <returns>The angle in the range [-Pi, Pi).</returns>
    public static float NormalizeAngleMinusPiToPi(float angle)
    {
        angle = (angle + MathF.PI) % TwoPi;
        if (angle < 0)
            angle += MathF.PI;
        return angle - MathF.PI;
    }

    public static float ClampAngle(float value, float min, float max)
    {
        value = WrapAngle(value);
        min = WrapAngle(min);
        max = WrapAngle(max);

        if (min < max)
            return Clamp(value, min, max);
        else
            return Clamp(value, max, min);
    }

    /// <summary>
    /// Linearly interpolates the angle in radians.
    /// </summary>
    /// <param name="a">The first angle in radians.</param>
    /// <param name="b">The second angle in radians.</param>
    /// <param name="t">The interpolation factor.</param>
    /// <returns>The interpolated angle in radians.</returns>
    public static float LerpAngle(float a, float b, float t)
    {
        float delta = AngleDifference(a, b);
        return a + delta * t;
    }

    /// <summary>
    /// Computes the 2d cross product of two vectors.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The cross product of the two vectors.</returns>
    public static float Cross(Vector2 a, Vector2 b)
    {
        return a.X * b.Y - a.Y * b.X;
    }

    /// <summary>
    /// Linearly interpolates between two vectors using a vector as the interpolation factor.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <param name="t">A vector containing the interpolation factors for each component.</param>
    /// <returns>The interpolated vector.</returns>
    public static Vector2 Lerp(Vector2 a, Vector2 b, Vector2 t)
    {
        return a + (b - a) * t;
    }
}
