using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace LifeSim;

public static class MathUtils
{
    private const float K_EPSILON = 0.00001f;
    private const float K_EPSILON_NORMAL_SQRT = 1e-15f;

    public static float PiOverTwo { get; } = (float)Math.PI / 2f;

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
    /// Calculates a weighted linearly interpolated value using 3 points to form a triangle and barycentric coordinates.
    /// This can be used to calculate a weighted value from a set of 3 points.
    /// For example when wanting to interpolate between 3 vertices of a triangle.
    /// </summary>
    /// <param name="a">The first point</param>
    /// <param name="b">The second point</param>
    /// <param name="c">The third point</param>
    /// <param name="hA">The value asociated with the first point</param>
    /// <param name="hB">The value asociated with the second point</param>
    /// <param name="hC">The value asociated with the third point</param>
    /// <param name="p">The point to calculate the value for</param>
    /// <returns>The weighted value</returns>
    public static float BarycentricLerp(Vector2 a, Vector2 b, Vector2 c, float hA, float hB, float hC, Vector2 p)
    {
        var v0 = b - a;
        var v1 = c - a;
        var v2 = p - a;
        var d00 = Vector2.Dot(v0, v0);
        var d01 = Vector2.Dot(v0, v1);
        var d11 = Vector2.Dot(v1, v1);
        var d20 = Vector2.Dot(v2, v0);
        var d21 = Vector2.Dot(v2, v1);
        var denom = d00 * d11 - d01 * d01;
        var v = (d11 * d20 - d01 * d21) / denom;
        var w = (d00 * d21 - d01 * d20) / denom;
        var u = 1 - v - w;
        return u * hA + v * hB + w * hC;
    }

    /// <summary>
    /// Extract the yaw, pitch and roll of a quaternion. This is the inverse of System.Numerics.Quaternion.CreateFromYawPitchRoll().
    /// </summary>
    /// <param name="q">The quaternion to get the yaw, pitch and roll from.</param>
    /// <param name="yaw">The yaw of the quaternion.</param>
    /// <param name="pitch">The pitch of the quaternion.</param>
    /// <param name="roll">The roll of the quaternion.</param>
    public static void GetYawPitchRoll(Quaternion q, out float yaw, out float pitch, out float roll)
    {
        // Source: https://stackoverflow.com/a/18115837/2022985

        // Get the yaw as the atan2 of the x and z components.
        yaw = MathF.Atan2(2.0f * (q.Y * q.Z + q.W * q.X), q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z);

        // Get the pitch as the asin of the x and w components.
        pitch = MathF.Asin(-2.0f * (q.X * q.Z - q.W * q.Y));

        // Get the roll as the atan2 of the y and z components.
        roll = MathF.Atan2(2.0f * (q.X * q.Y + q.W * q.Z), q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z);
    }
}