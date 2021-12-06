using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace LifeSim
{
    public static class MathUtils
    {
        private const float K_EPSILON = 0.00001f;
        private const float K_EPSILON_NORMAL_SQRT = 1e-15f;

        public static float MoveTowardsAngle(float current, float target, float maxDelta)
        {
            float deltaAngle = DeltaAngle(current, target);
            if (-maxDelta < deltaAngle && deltaAngle < maxDelta)
                return target;
            target = current + deltaAngle;
            return MoveTowards(current, target, maxDelta);
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

        // Returns the angle in radians between /from/ and /to/.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(Vector2 from, Vector2 to)
        {
            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            float denominator = (float)MathF.Sqrt(from.LengthSquared() * to.LengthSquared());
            if (denominator < K_EPSILON_NORMAL_SQRT)
                return 0F;

            float dot = MathF.Max(-1f, Math.Min(1f, Vector2.Dot(from, to) / denominator));
            return MathF.Acos(dot);
        }


        /// <summary>
        /// Returns an unsigned integer containing 32 reasonably-well-scrambled
        /// bits, based on a given (signed) integer input parameter `n` and optional
        /// `seed`.  Kind of like looking up a value in an infinitely large
        /// non-existent table of previously generated random numbers.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
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
        /// <param name="quaternion">The quaternion</param>
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
    }
}