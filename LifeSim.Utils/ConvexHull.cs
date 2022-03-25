using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace LifeSim;

public static class ConvexHull
{
    public static List<Vector2> GetConvexHull(List<Vector2> points)
    {
        if (points.Count <= 1)
            return points;

        int n = points.Count, k = 0;
        List<Vector2> H = new List<Vector2>(2 * n);

        points.Sort((a, b) =>
             a.X == b.X ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));

        // Build lower hull
        for (int i = 0; i < n; ++i)
        {
            while (k >= 2 && Cross(H[k - 2], H[k - 1], points[i]) <= 0)
                k--;
            H[k++] = points[i];
        }

        // Build upper hull
        for (int i = n - 2, t = k + 1; i >= 0; i--)
        {
            while (k >= t && Cross(H[k - 2], H[k - 1], points[i]) <= 0)
                k--;
            H[k++] = points[i];
        }

        return H.Take(k - 1).ToList();
    }

    private static float Cross(Vector2 o, Vector2 a, Vector2 b)
    {
        return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
    }
}