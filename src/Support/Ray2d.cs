using System.Numerics;

namespace Support;

/// <summary>
/// Represents a 2d ray.
/// </summary>
public struct Ray2d
{
    /// <summary>
    /// Gets or sets the origin of the ray.
    /// </summary>
    public Vector2 Origin { get; set; }

    /// <summary>
    /// Gets or sets the direction of the ray. This vector is expected to be normalized.
    /// </summary>
    public Vector2 Direction { get; set; }

    /// <summary>
    /// Constructs a new ray.
    /// </summary>
    /// <param name="origin">The origin of the ray.</param>
    /// <param name="direction">The direction of the ray.</param>
    public Ray2d(Vector2 origin, Vector2 direction)
    {
        this.Origin = origin;
        this.Direction = direction;
    }

    /// <summary>
    /// Gets a point on the ray at the specified distance.
    /// </summary>
    /// <param name="distance">The distance from the origin.</param>
    /// <returns>The point on the ray at the specified distance.</returns>
    public Vector2 GetPoint(float distance)
    {
        return this.Origin + this.Direction * distance;
    }
}
