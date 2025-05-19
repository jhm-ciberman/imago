using System.Numerics;

namespace LifeSim.Support.Numerics;

public static class Distance
{
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
        float dx = float.Abs(v.X);
        float dy = float.Abs(v.Y);
        float diagonal = float.Min(dx, dy);
        float orthogonal = float.Abs(dx - dy);
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
        return float.Abs(v.X) + float.Abs(v.Y);
    }
}
