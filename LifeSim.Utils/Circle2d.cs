using System;
using System.Numerics;

namespace LifeSim;

/// <summary>
/// Represents a circle in 2D space.
/// </summary>
public struct Circle2d
{
    /// <summary>
    /// Gets or sets the center of the circle.
    /// </summary>
    public Vector2 Center { get; set; }

    /// <summary>
    /// Gets or sets the radius of the circle.
    /// </summary>
    public float Radius { get; set; }

    /// <summary>
    /// Constructs a new circle.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    public Circle2d(Vector2 center, float radius)
    {
        this.Center = center;
        this.Radius = radius;
    }

    /// <summary>
    /// Returns whether the circle contains the given point.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <returns>Whether the circle contains the given point.</returns>
    public bool Contains(Vector2 point)
    {
        return (point - this.Center).LengthSquared() <= this.Radius * this.Radius;
    }

    /// <summary>
    /// Returns whether the circle intersects the given bounding box.
    /// </summary>
    /// <param name="box">The bounding box to test.</param>
    /// <returns>Whether the circle intersects the given bounding box.</returns>
    public bool Intersects(BoundingBox2d box)
    {
        return (box.Min - this.Center).LengthSquared() <= this.Radius * this.Radius ||
            (box.Max - this.Center).LengthSquared() <= this.Radius * this.Radius;
    }

    /// <summary>
    /// Returns whether the circle intersects the given ray and returns the intersection distance.
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="distance">The distance along the ray to the intersection.</param>
    /// <returns>Whether the circle intersects the given ray.</returns>
    public bool Intersects(Ray2d ray, out float distance)
    {
        float a = ray.Direction.LengthSquared();
        float b = 2 * Vector2.Dot(ray.Direction, ray.Origin - this.Center);
        float c = (ray.Origin - this.Center).LengthSquared() - this.Radius * this.Radius;
        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            distance = 0;
            return false;
        }
        else
        {
            float t = (-b - MathF.Sqrt(discriminant)) / (2 * a);
            if (t < 0)
            {
                // This means the collision point is behind the ray origin.
                distance = 0f;
                return false;
            }
            distance = t;
            return true;
        }
    }

    /// <summary>
    /// Returns whether the circle intersects the given ray.
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <returns>Whether the circle intersects the given ray.</returns>
    public bool Intersects(Ray2d ray)
    {
        return this.Intersects(ray, out _);
    }

}