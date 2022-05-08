using System;
using System.Diagnostics;
using System.Numerics;

namespace LifeSim;

/// <summary>
/// Represents a Bounding box in 2d space.
/// </summary>
public struct BoundingBox2d
{
    public static BoundingBox2d Empty => new BoundingBox2d(Vector2.Zero, Vector2.Zero);

    /// <summary>
    /// Gets or sets the minimum point of the box.
    /// </summary>
    public Vector2 Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum point of the box.
    /// </summary>
    public Vector2 Max { get; set; }

    /// <summary>
    /// Gets the size of the box.
    /// </summary>
    public Vector2 Size => this.Max - this.Min;

    /// <summary>
    /// Gets the center of the box.
    /// </summary>
    public Vector2 Center => (this.Min + this.Max) / 2;

    /// <summary>
    /// Constructs a new bounding box.
    /// </summary>
    /// <param name="min">The minimum point of the box.</param>
    /// <param name="max">The maximum point of the box.</param>
    public BoundingBox2d(Vector2 min, Vector2 max)
    {
        Debug.Assert(min.X <= max.X && min.Y <= max.Y, "The minimum point must be less than or equal to the maximum point.");
        this.Min = min;
        this.Max = max;
    }

    /// <summary>
    /// Constructs a new bounding box.
    /// </summary>
    /// <param name="minX">The minimum x-coordinate of the box.</param>
    /// <param name="minY">The minimum y-coordinate of the box.</param>
    /// <param name="maxX">The maximum x-coordinate of the box.</param>
    /// <param name="maxY">The maximum y-coordinate of the box.</param>
    public BoundingBox2d(float minX, float minY, float maxX, float maxY)
    {
        Debug.Assert(minX <= maxX && minY <= maxY, "The minimum point must be less than or equal to the maximum point.");
        this.Min = new Vector2(minX, minY);
        this.Max = new Vector2(maxX, maxY);
    }

    /// <summary>
    /// Returns whether the bounding box contains the given point.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <returns>Whether the bounding box contains the given point.</returns>
    public bool Contains(Vector2 point)
    {
        return point.X >= this.Min.X && point.X <= this.Max.X && point.Y >= this.Min.Y && point.Y <= this.Max.Y;
    }

    /// <summary>
    /// Returns whether the bounding box intersects the given bounding box.
    /// </summary>
    /// <param name="other">The bounding box to test.</param>
    /// <returns>Whether the bounding box intersects the given bounding box.</returns>
    public bool Intersects(BoundingBox2d other)
    {
        return this.Min.X <= other.Max.X && this.Max.X >= other.Min.X && this.Min.Y <= other.Max.Y && this.Max.Y >= other.Min.Y;
    }

    /// <summary>
    /// Returns whether the bounding box intersects the given bounding box.
    /// </summary>
    /// <param name="min">The minimum point of the bounding box to test.</param>
    /// <param name="max">The maximum point of the bounding box to test.</param>
    /// <returns>Whether the bounding box intersects the given bounding box.</returns>
    public bool Intersects(Vector2 min, Vector2 max)
    {
        return this.Min.X <= max.X && this.Max.X >= min.X && this.Min.Y <= max.Y && this.Max.Y >= min.Y;
    }

    /// <summary>
    /// Returns whether the bounding box intersects the given bounding box and returns the intersection.
    /// </summary>
    /// <param name="other">The bounding box to test.</param>
    /// <param name="intersection">The intersection of the two bounding boxes.</param>
    /// <returns>Whether the bounding box intersects the given bounding box.</returns>
    public bool Intersects(BoundingBox2d other, out BoundingBox2d intersection)
    {
        if (this.Min.X > other.Max.X || this.Max.X < other.Min.X || this.Min.Y > other.Max.Y || this.Max.Y < other.Min.Y)
        {
            intersection = new BoundingBox2d(Vector2.Zero, Vector2.Zero);
            return false;
        }
        intersection = new BoundingBox2d(
            MathF.Max(this.Min.X, other.Min.X), MathF.Max(this.Min.Y, other.Min.Y),
            MathF.Min(this.Max.X, other.Max.X), MathF.Min(this.Max.Y, other.Max.Y));
        return true;
    }

    /// <summary>
    /// Returns whether the bounding box intersects the given ray.
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="distance">The distance along the ray to the intersection.</param>
    /// <returns>Whether the bounding box intersects the given ray.</returns>
    public bool Intersects(Ray2d ray, out float distance)
    {
        float min = 0;
        float max = float.MaxValue;
        if (ray.Direction.X != 0)
        {
            float inverse = 1 / ray.Direction.X;
            float minX = (this.Min.X - ray.Origin.X) * inverse;
            float maxX = (this.Max.X - ray.Origin.X) * inverse;
            if (minX > maxX)
            {
                (maxX, minX) = (minX, maxX);
            }
            min = MathF.Max(min, minX);
            max = MathF.Min(max, maxX);
            if (min > max)
            {
                distance = 0;
                return false;
            }
        }
        if (ray.Direction.Y != 0)
        {
            float inverse = 1 / ray.Direction.Y;
            float minY = (this.Min.Y - ray.Origin.Y) * inverse;
            float maxY = (this.Max.Y - ray.Origin.Y) * inverse;
            if (minY > maxY)
            {
                (maxY, minY) = (minY, maxY);
            }
            min = MathF.Max(min, minY);
            max = MathF.Min(max, maxY);
            if (min > max)
            {
                distance = 0;
                return false;
            }
        }

        distance = min;
        return true;
    }

    /// <summary>
    /// Returns whether the bounding box intersects the given ray.
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <returns>Whether the bounding box intersects the given ray.</returns>
    public bool Intersects(Ray2d ray)
    {
        return this.Intersects(ray, out _);
    }
}
