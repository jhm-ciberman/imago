using System;
using System.Numerics;

namespace LifeSim.Support.Numerics;

public static class MathUtils
{
    public static float AngleDifference(float current, float target)
    {
        float delta = (target - current) % (MathF.PI * 2);

        if (delta > MathF.PI)
            delta -= MathF.PI * 2;
        else if (delta < -MathF.PI)
            delta += MathF.PI * 2;

        return delta;
    }

    /// <summary>
    /// Returns the angle in radians between 0 and 2*Pi.
    /// </summary>
    /// <param name="value">The angle in radians.</param>
    /// <returns>The angle in the range [0, 2*Pi).</returns>
    public static float WrapAngle(float value)
    {
        return (value + MathF.PI * 2f) % (MathF.PI * 2f);
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
    /// Computes the octile distance between two points. This is the distance between two points if only moving
    /// diagonally or orthogonally is allowed.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>The octile distance between the two points.</returns>
    public static float OctileDistance(Vector2 a, Vector2 b)
    {
        Vector2 v = a - b;
        float dx = Math.Abs(v.X);
        float dy = Math.Abs(v.Y);
        float diagonal = Math.Min(dx, dy);
        float orthogonal = Math.Abs(dx - dy);
        return orthogonal + diagonal * 1.4f;
    }

    /// <summary>
    /// Computes the Manhattan distance between two points. This is the distance between two points if only moving
    /// orthogonally is allowed.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>The Manhattan distance between the two points.</returns>
    public static float ManhattanDistance(Vector2 a, Vector2 b)
    {
        Vector2 v = a - b;
        return Math.Abs(v.X) + Math.Abs(v.Y);
    }
}
