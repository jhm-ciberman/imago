using System.Numerics;

namespace Imago.Utilities;

/// <summary>
/// Represents the eight corner points of a viewing frustum.
/// </summary>
public struct FrustumCorners
{
    /// <summary>
    /// Gets or sets the top-left corner of the near plane.
    /// </summary>
    public Vector3 NearTopLeft;

    /// <summary>
    /// Gets or sets the top-right corner of the near plane.
    /// </summary>
    public Vector3 NearTopRight;

    /// <summary>
    /// Gets or sets the bottom-left corner of the near plane.
    /// </summary>
    public Vector3 NearBottomLeft;

    /// <summary>
    /// Gets or sets the bottom-right corner of the near plane.
    /// </summary>
    public Vector3 NearBottomRight;

    /// <summary>
    /// Gets or sets the top-left corner of the far plane.
    /// </summary>
    public Vector3 FarTopLeft;

    /// <summary>
    /// Gets or sets the top-right corner of the far plane.
    /// </summary>
    public Vector3 FarTopRight;

    /// <summary>
    /// Gets or sets the bottom-left corner of the far plane.
    /// </summary>
    public Vector3 FarBottomLeft;

    /// <summary>
    /// Gets or sets the bottom-right corner of the far plane.
    /// </summary>
    public Vector3 FarBottomRight;
}
