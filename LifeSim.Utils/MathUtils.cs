using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace LifeSim;

public static class MathUtils
{
    private const float K_EPSILON = 0.00001f;
    private const float K_EPSILON_NORMAL_SQRT = 1e-15f;

    public static float HalfPi { get; } = (float)Math.PI / 2f;

    public static float TwoPi { get; } = (float)Math.PI * 2f;

    /// <summary>
    /// Linearly interpolates between two values.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <param name="t">The interpolation factor.</param>
    /// <returns>The interpolated value.</returns>
    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    public static float MoveTowardsAngle(float current, float target, float maxDelta)
    {
        float deltaAngle = DeltaAngle(current, target);
        if (-maxDelta < deltaAngle && deltaAngle < maxDelta)
            return target;
        target = current + deltaAngle;
        return MoveTowards(current, target, maxDelta);
    }

    /// <summary>
    /// Smoothly interpolates between two values.
    /// </summary>
    /// <param name="v1">The first value.</param>
    /// <param name="v2">The second value.</param>
    /// <param name="lerpProgress">The interpolation factor.</param>
    /// <returns>The interpolated value.</returns>
    public static float SmoothStep(float v1, float v2, float lerpProgress)
    {
        return Lerp(v1, v2, lerpProgress * lerpProgress * (3f - 2f * lerpProgress));
    }

    // Moves a value /current/ towards /target/.
    public static float MoveTowards(float current, float target, float maxDelta)
    {
        if (MathF.Abs(target - current) <= maxDelta)
            return target;
        return current + MathF.Sign(target - current) * maxDelta;
    }

    public static float DeltaAngle(float current, float target)
    {
        float delta = (target - current) % (MathF.PI * 2);
        if (delta > MathF.PI)
            delta -= (MathF.PI * 2);
        return delta;
    }

    /// <summary>
    /// Returns the angle difference in radians between /from/ and /to/.
    /// </summary>
    /// <param name="from">The source angle in radians.</param>
    /// <param name="to">The target angle in radians.</param>
    /// <returns>The delta angle in radians.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float AngleDifference(Vector2 from, Vector2 to)
    {
        // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
        float denominator = (float)MathF.Sqrt(from.LengthSquared() * to.LengthSquared());
        if (denominator < K_EPSILON_NORMAL_SQRT)
            return 0F;

        float dot = MathF.Max(-1f, Math.Min(1f, Vector2.Dot(from, to) / denominator));
        return MathF.Acos(dot);
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
    /// Returns the angle in radians between -Pi and Pi.
    /// </summary>
    /// <param name="2f">The angle in radians.</param>
    /// <returns>The angle in the range -Pi to Pi.</returns>
    public static float WrapAngle(float value)
    {
        value %= MathF.PI * 2f;
        if (value < 0f)
            value += MathF.PI * 2f;
        return value;
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
        float delta = DeltaAngle(a, b);
        return a + delta * t;
    }


    /// <summary>
    /// Returns an integer containing 32 reasonably-well-scrambled
    /// bits, based on a given (signed) integer input parameter `n` and optional
    /// `seed`.  Kind of like looking up a value in an infinitely large
    /// non-existent table of previously generated random numbers.
    /// </summary>
    /// <param name="n">The integer to scramble.</param>
    /// <param name="seed">An optional seed to use.</param>
    /// <returns>A random integer.</returns>
    public static int Squirrel3(int n, int seed)
    {
        unchecked
        {
            const int NOISE1 = (int) 0xb5297a4d;
            const int NOISE2 = (int) 0x68e31da4;
            const int NOISE3 = (int) 0x1b56c4e9;
            n *= NOISE1;
            n += seed;
            n ^= n >> 8;
            n += NOISE2;
            n ^= n << 8;
            n *= NOISE3;
            n ^= n >> 8;
            return n;
        }
    }


    /// <summary>
    /// Converts a Quaternion to a Vector3 representing the euler angles in a right handed coordinate system.
    /// </summary>
    /// <param name="q">The quaternion</param>
    /// <returns>The euler angles in a right handed coordinate system</returns>
    public static Vector3 QuaternionToEuler(Quaternion q)
    {
        Vector3 euler;

        // if the input quaternion is normalized, this is exactly one. Otherwise, this acts as a correction factor for the quaternion's not-normalizedness
        float unit = (q.X * q.X) + (q.Y * q.Y) + (q.Z * q.Z) + (q.W * q.W);

        // this will have a magnitude of 0.5 or greater if and only if this is a singularity case
        float test = q.X * q.W - q.Y * q.Z;

        if (test > 0.4995f * unit) // singularity at north pole
        {
            euler.X = MathF.PI / 2;
            euler.Y = 2f * MathF.Atan2(q.Y, q.X);
            euler.Z = 0;
        }
        else if (test < -0.4995f * unit) // singularity at south pole
        {
            euler.X = -MathF.PI / 2;
            euler.Y = -2f * MathF.Atan2(q.Y, q.X);
            euler.Z = 0;
        }
        else // no singularity - this is the majority of cases
        {
            euler.X = MathF.Asin(2f * (q.W * q.X - q.Y * q.Z));
            euler.Y = MathF.Atan2(2f * q.W * q.Y + 2f * q.Z * q.X, 1 - 2f * (q.X * q.X + q.Y * q.Y));
            euler.Z = MathF.Atan2(2f * q.W * q.Z + 2f * q.X * q.Y, 1 - 2f * (q.Z * q.Z + q.X * q.X));
        }

        // Transform from left handed to right handed coordinate system
        euler.X = -euler.X;
        euler.Z = -euler.Z;

        return euler;
    }

    /// <summary>
    /// Computes a ray to triangle intersection.
    /// </summary>
    /// <param name="rayOrigin">The ray origin.</param>
    /// <param name="rayDirection">The ray direction.</param>
    /// <param name="vertexA">The first vertex of the triangle.</param>
    /// <param name="vertexB">The second vertex of the triangle.</param>
    /// <param name="vertexC">The third vertex of the triangle.</param>
    /// <returns>True if the ray intersects the triangle, false otherwise.</returns>
    public static bool RayTriangleIntersection(Vector3 rayOrigin, Vector3 rayDirection, Vector3 vertexA, Vector3 vertexB, Vector3 vertexC)
    {
        // Computes the ray to triangle intersection 
        // https://cadxfem.org/inf/Fast%20MinimumStorage%20RayTriangle%20Intersection.pdf

        // Compute vectors along two edges of the triangle.
        Vector3 edge1 = vertexB - vertexA;
        Vector3 edge2 = vertexC - vertexA;

        // Begin calculating determinant - also used to calculate U parameter.
        Vector3 pvec = Vector3.Cross(rayDirection, edge2);

        // If determinant is near zero, ray lies in plane of triangle.
        float det = Vector3.Dot(edge1, pvec);

        // NOT CULLING
        if (det > -MathUtils.K_EPSILON && det < MathUtils.K_EPSILON)
            return false;

        float inv_det = 1f / det;

        // Calculate distance from vert0 to ray origin.
        Vector3 tvec = rayOrigin - vertexA;

        // Calculate U parameter and test bounds.
        float u = Vector3.Dot(tvec, pvec) * inv_det;

        // The intersection lies outside of the triangle.
        if (u < 0 || u > 1)
            return false;

        // Prepare to test V parameter.
        Vector3 qvec = Vector3.Cross(tvec, edge1);

        // Calculate V parameter and test bounds.
        float v = Vector3.Dot(rayDirection, qvec) * inv_det;

        // The intersection lies outside of the triangle.
        if (v < 0 || u + v > 1)
            return false;

        // Ray intersects triangle.
        return true;
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
    /// Computes the intersection of two 2d segments.
    /// </summary>
    /// <param name="aStart">The start of the first segment.</param>
    /// <param name="aEnd">The end of the first segment.</param>
    /// <param name="bStart">The start of the second segment.</param>
    /// <param name="bEnd">The end of the second segment.</param>
    /// <param name="intersection">The intersection of the two segments.</param>
    /// <returns>True if the segments intersect, false otherwise.</returns>
    public static bool SegmentToSegmentIntersection(Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd, out Vector2 intersection)
    {
        // Source: https://stackoverflow.com/a/1968345/2022985

        var s1 = new Vector2(aEnd.X - aStart.X, aEnd.Y - aStart.Y);
        var s2 = new Vector2(bEnd.X - bStart.X, bEnd.Y - bStart.Y);

        var s = (-s1.Y * (aStart.X - bStart.X) + s1.X * (aStart.Y - bStart.Y)) / (-s2.X * s1.Y + s1.X * s2.Y);
        var t = (s2.X * (aStart.Y - bStart.Y) - s2.Y * (aStart.X - bStart.X)) / (-s2.X * s1.Y + s1.X * s2.Y);

        if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
        {
            // Collision detected
            intersection.X = aStart.X + (t * s1.X);
            intersection.Y = aStart.Y + (t * s1.Y);
            return true;
        }

        intersection = Vector2.Zero;
        return false; // No collision
    }


    /// <summary>
    /// Calculates the point of intersection between a 2d segment and a 2d triangle. Returns false if the ray does not intersect the triangle.
    /// </summary>
    /// <param name="rayStart">The start of the ray.</param>
    /// <param name="rayEnd">The end  of the ray.</param>
    /// <param name="pA">The first vertex of the triangle.</param>
    /// <param name="pB">The second vertex of the triangle.</param>
    /// <param name="pC">The third vertex of the triangle.</param>
    /// <param name="intersectionPoint">The point of intersection.</param>
    /// <returns>True if the ray intersects the triangle, false otherwise.</returns>
    public static bool SegmentToTriangleIntersection(Vector2 rayStart, Vector2 rayEnd, Vector2 pA, Vector2 pB, Vector2 pC, out Vector2 intersectionPoint)
    {
        return SegmentToSegmentIntersection(rayStart, rayEnd, pA, pB, out intersectionPoint)
         || SegmentToSegmentIntersection(rayStart, rayEnd, pB, pC, out intersectionPoint)
         || SegmentToSegmentIntersection(rayStart, rayEnd, pC, pA, out intersectionPoint);
    }

    /// <summary>
    /// Computes the point of intersection between a 2d segment and a circle. Returns false if the ray does not intersect the circle.
    /// </summary>
    /// <param name="rayStart">The start of the ray.</param>
    /// <param name="rayEnd">The end  of the ray.</param>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="intersectionPoint">The point of intersection.</param>
    /// <returns>True if the ray intersects the circle, false otherwise.</returns>
    public static bool SegmentToCircleIntersection(Vector2 rayStart, Vector2 rayEnd, Vector2 circleCenter, float radius, out Vector2 intersectionPoint)
    {
        // Source: https://stackoverflow.com/a/1968345/2022985

        var d = rayEnd - rayStart;
        var f = rayStart - circleCenter;

        var a = Vector2.Dot(d, d);
        var b = 2 * Vector2.Dot(f, d);
        var c = Vector2.Dot(f, f) - radius * radius;

        var discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            // No intersection
            intersectionPoint = Vector2.Zero;
            return false;
        }

        var t = (-b - MathF.Sqrt(discriminant)) / (2 * a);
        if (t < 0 || t > 1)
        {
            // No intersection
            intersectionPoint = Vector2.Zero;
            return false;
        }

        intersectionPoint = rayStart + t * d;
        return true;
    }


    // Triangle to circle intersection

    /// <summary>
    /// Computes the point of intersection between a circle and a 2d triangle. Returns false if the triangle does not intersect the circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="pA">The first vertex of the triangle.</param>
    /// <param name="pB">The second vertex of the triangle.</param>
    /// <param name="pC">The third vertex of the triangle.</param>
    /// <param name="intersectionPoint">The point of intersection.</param>
    /// <returns>True if the triangle intersects the circle, false otherwise.</returns>
    public static bool CircleToTriangleIntersection(Vector2 circleCenter, float radius, Vector2 pA, Vector2 pB, Vector2 pC, out Vector2 intersectionPoint)
    {
        intersectionPoint = Vector2.Zero;
        return false;
    }

    /// <summary>
    /// Projects a point onto a vector.
    /// </summary>
    /// <param name="point">The point to project.</param>
    /// <param name="axis">The vector to project onto.</param>
    /// <returns>The projected point.</returns>
    public static Vector2 ProjectPoint(Vector2 point, Vector2 axis)
    {
        return Vector2.Dot(point, axis) * axis;
    }
}