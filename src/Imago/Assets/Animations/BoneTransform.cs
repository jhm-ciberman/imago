using System.Numerics;

namespace Imago.Assets.Animations;

/// <summary>
/// Represents the local transform of a single bone as a position, rotation, and scale.
/// </summary>
public struct BoneTransform
{
    /// <summary>
    /// Gets or sets the local position.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// Gets or sets the local rotation.
    /// </summary>
    public Quaternion Rotation;

    /// <summary>
    /// Gets or sets the local scale.
    /// </summary>
    public Vector3 Scale;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoneTransform"/> struct.
    /// </summary>
    /// <param name="position">The local position.</param>
    /// <param name="rotation">The local rotation.</param>
    /// <param name="scale">The local scale.</param>
    public BoneTransform(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        this.Position = position;
        this.Rotation = rotation;
        this.Scale = scale;
    }

    /// <summary>
    /// Gets a transform with zero position, identity rotation, and unit scale.
    /// </summary>
    public static BoneTransform Identity => new BoneTransform(Vector3.Zero, Quaternion.Identity, Vector3.One);

    /// <summary>
    /// Linearly interpolates two bone transforms component-wise.
    /// </summary>
    /// <remarks>
    /// Position and scale use linear interpolation; rotation uses spherical linear interpolation.
    /// </remarks>
    /// <param name="a">The transform at <paramref name="t"/> = 0.</param>
    /// <param name="b">The transform at <paramref name="t"/> = 1.</param>
    /// <param name="t">The interpolation factor in the [0, 1] range.</param>
    /// <returns>The interpolated transform.</returns>
    public static BoneTransform Lerp(in BoneTransform a, in BoneTransform b, float t)
    {
        return new BoneTransform(
            Vector3.Lerp(a.Position, b.Position, t),
            Quaternion.Slerp(a.Rotation, b.Rotation, t),
            Vector3.Lerp(a.Scale, b.Scale, t)
        );
    }
}
