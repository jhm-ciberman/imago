using System;
using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Imago.Utilities;

/// <summary>
/// Represents a bounding sphere in 3D space.
/// </summary>
public struct BoundingSphere
{
    /// <summary>
    /// The center point of the sphere.
    /// </summary>
    public Vector3 Center;

    /// <summary>
    /// The radius of the sphere.
    /// </summary>
    public float Radius;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundingSphere"/> struct.
    /// </summary>
    /// <param name="center">The center point of the sphere.</param>
    /// <param name="radius">The radius of the sphere.</param>
    public BoundingSphere(Vector3 center, float radius)
    {
        this.Center = center;
        this.Radius = radius;
    }

    public override string ToString()
    {
        return string.Format("Center:{0}, Radius:{1}", this.Center, this.Radius);
    }

    /// <summary>
    /// Checks whether the bounding sphere contains the specified point.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <returns>True if the point is inside the sphere; otherwise, false.</returns>
    public bool Contains(Vector3 point)
    {
        return (this.Center - point).LengthSquared() <= this.Radius * this.Radius;
    }

    /// <summary>
    /// Creates a new bounding sphere that contains a list of points.
    /// </summary>
    /// <param name="points">The list of points to contain.</param>
    /// <returns>The created bounding sphere.</returns>
    public static BoundingSphere CreateFromPoints(IList<Vector3> points)
    {
        Vector3 center = Vector3.Zero;
        foreach (Vector3 pt in points)
        {
            center += pt;
        }

        center /= points.Count;

        float maxDistanceSquared = 0f;
        foreach (Vector3 pt in points)
        {
            float distSq = Vector3.DistanceSquared(center, pt);
            if (distSq > maxDistanceSquared)
                maxDistanceSquared = distSq;
        }

        return new BoundingSphere(center, (float)Math.Sqrt(maxDistanceSquared));
    }

    /// <summary>
    /// Creates a new bounding sphere that contains a set of points.
    /// </summary>
    /// <param name="pointPtr">A pointer to the first point.</param>
    /// <param name="numPoints">The number of points.</param>
    /// <param name="stride">The stride between points in bytes.</param>
    /// <returns>The created bounding sphere.</returns>
    public static unsafe BoundingSphere CreateFromPoints(Vector3* pointPtr, int numPoints, int stride)
    {
        Vector3 center = Vector3.Zero;
        StrideHelper<Vector3> helper = new StrideHelper<Vector3>(pointPtr, numPoints, stride);
        foreach (Vector3 pos in helper)
        {
            center += pos;
        }

        center /= numPoints;

        float maxDistanceSquared = 0f;
        foreach (Vector3 pos in helper)
        {
            float distSq = Vector3.DistanceSquared(center, pos);
            if (distSq > maxDistanceSquared)
                maxDistanceSquared = distSq;
        }

        return new BoundingSphere(center, (float)Math.Sqrt(maxDistanceSquared));
    }
}
