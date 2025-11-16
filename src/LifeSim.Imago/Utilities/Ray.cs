using System.Numerics;

namespace LifeSim.Imago.Utilities;

/// <summary>
/// Represents a ray in 3D space, defined by an origin and a direction.
/// </summary>
public struct Ray
{
    /// <summary>
    /// The origin point of the ray.
    /// </summary>
    public Vector3 Origin;

    /// <summary>
    /// The normalized direction vector of the ray.
    /// </summary>
    public Vector3 Direction;

    /// <summary>
    /// Initializes a new instance of the <see cref="Ray"/> struct.
    /// </summary>
    /// <param name="origin">The origin point of the ray.</param>
    /// <param name="direction">The direction vector of the ray.</param>
    public Ray(Vector3 origin, Vector3 direction)
    {
        this.Origin = origin;
        this.Direction = direction;
    }

    /// <summary>
    /// Checks whether the ray intersects with a bounding box.
    /// </summary>
    /// <param name="box">The bounding box to check.</param>
    /// <returns>True if the ray intersects the box; otherwise, false.</returns>
    public bool Intersects(BoundingBox box)
    {
        return this.Intersects(ref box);
    }

    /// <summary>
    /// Checks whether the ray intersects with a bounding box.
    /// </summary>
    /// <param name="box">The bounding box to check.</param>
    /// <returns>True if the ray intersects the box; otherwise, false.</returns>
    public bool Intersects(ref BoundingBox box)
    {
        // http://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-box-intersection

        float tmin = (box.Min.X - this.Origin.X) / this.Direction.X;
        float tmax = (box.Max.X - this.Origin.X) / this.Direction.X;

        if (tmin > tmax)
            Swap(ref tmin, ref tmax);

        float tymin = (box.Min.Y - this.Origin.Y) / this.Direction.Y;
        float tymax = (box.Max.Y - this.Origin.Y) / this.Direction.Y;

        if (tymin > tymax)
            Swap(ref tymin, ref tymax);

        if (tmin > tymax || tymin > tmax)
            return false;

        if (tymin > tmin)
            tmin = tymin;

        if (tymax < tmax)
            tmax = tymax;

        float tzmin = (box.Min.Z - this.Origin.Z) / this.Direction.Z;
        float tzmax = (box.Max.Z - this.Origin.Z) / this.Direction.Z;

        if (tzmin > tzmax)
            Swap(ref tzmin, ref tzmax);

        if (tmin > tzmax || tzmin > tmax)
            return false;

        if (tzmin > tmin)
            tmin = tzmin;

        if (tzmax < tmax)
            tmax = tzmax;

        return true;
    }

    private static void Swap(ref float a, ref float b)
    {
        (b, a) = (a, b);
    }

    /// <summary>
    /// Creates a new ray that is transformed by the given matrix.
    /// </summary>
    /// <param name="ray">The original ray.</param>
    /// <param name="mat">The transformation matrix.</param>
    /// <returns>The transformed ray.</returns>
    public static Ray Transform(Ray ray, Matrix4x4 mat)
    {
        return new Ray(Vector3.Transform(ray.Origin, mat), Vector3.Normalize(Vector3.TransformNormal(ray.Direction, mat)));
    }

    /// <summary>
    /// Checks whether the ray intersects with a triangle.
    /// </summary>
    /// <remarks>
    /// This method uses the Möller–Trumbore intersection algorithm.
    /// </remarks>
    /// <param name="V1">The first vertex of the triangle.</param>
    /// <param name="V2">The second vertex of the triangle.</param>
    /// <param name="V3">The third vertex of the triangle.</param>
    /// <param name="distance">When this method returns, contains the distance from the ray origin to the intersection point, if an intersection occurred.</param>
    /// <returns>True if the ray intersects the triangle; otherwise, false.</returns>
    public bool Intersects(ref Vector3 V1, ref Vector3 V2, ref Vector3 V3, out float distance)
    {
        const float Epsilon = 1E-6f;

        Vector3 e1, e2;  //Edge1, Edge2
        Vector3 P, Q, T;
        float det, inv_det, u, v;
        float t;

        //Find vectors for two edges sharing V1
        e1 = V2 - V1;
        e2 = V3 - V1;
        //Begin calculating determinant - also used to calculate u parameter
        P = Vector3.Cross(this.Direction, e2);
        //if determinant is near zero, ray lies in plane of triangle or ray is parallel to plane of triangle
        det = Vector3.Dot(e1, P);
        //NOT CULLIN
        if (det > -Epsilon && det < Epsilon)
        {
            distance = 0f;
            return false;
        }

        inv_det = 1.0f / det;

        //calculate distance from V1 to ray origin
        T = this.Origin - V1;

        //Calculate u parameter and test bound
        u = Vector3.Dot(T, P) * inv_det;
        //The intersection lies outside of the triangle
        if (u < 0.0f || u > 1.0f)
        {
            distance = 0f;
            return false;
        }

        //Prepare to test v parameter
        Q = Vector3.Cross(T, e1);

        //Calculate V parameter and test bound
        v = Vector3.Dot(this.Direction, Q) * inv_det;
        //The intersection lies outside of the triangle
        if (v < 0.0f || u + v > 1.0f)
        {
            distance = 0f;
            return false;
        }

        t = Vector3.Dot(e2, Q) * inv_det;

        if (t > Epsilon)
        { //ray intersection
            distance = t;
            return true;
        }

        // No hit, no win
        distance = 0f;
        return false;
    }
}
