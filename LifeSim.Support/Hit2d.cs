using System.Numerics;

namespace LifeSim.Support;


/// <summary>
/// Represents a collision hit result.
/// </summary>
public struct Hit2d
{
    /// <summary>
    /// Gets the time of impact. This value ranges from 0 to 1. A value greater than 1 means that the ray has not hit anything.
    /// </summary>
    public float Time { get; }

    /// <summary>
    /// Gets the normal of the surface at the point of impact.
    /// </summary>
    public Vector2 Normal { get; }

    /// <summary>
    /// Creates a new <see cref="Hit2d"/> instance.
    /// </summary>
    /// <param name="time">The time of impact.</param>
    /// <param name="normal">The normal of the surface at the point of impact.</param>
    public Hit2d(float time, Vector2 normal)
    {
        this.Time = time;
        this.Normal = normal;
    }
}
