using System.Numerics;

namespace LifeSim.Imago.Utilities;

/// <summary>
/// Represents the eight corner points of an axis-aligned bounding box.
/// </summary>
public struct AlignedBoxCorners
{
    /// <summary>
    /// Gets or sets the top-left corner of the near face.
    /// </summary>
    public Vector3 NearTopLeft;

    /// <summary>
    /// Gets or sets the top-right corner of the near face.
    /// </summary>
    public Vector3 NearTopRight;

    /// <summary>
    /// Gets or sets the bottom-left corner of the near face.
    /// </summary>
    public Vector3 NearBottomLeft;

    /// <summary>
    /// Gets or sets the bottom-right corner of the near face.
    /// </summary>
    public Vector3 NearBottomRight;

    /// <summary>
    /// Gets or sets the top-left corner of the far face.
    /// </summary>
    public Vector3 FarTopLeft;

    /// <summary>
    /// Gets or sets the top-right corner of the far face.
    /// </summary>
    public Vector3 FarTopRight;

    /// <summary>
    /// Gets or sets the bottom-left corner of the far face.
    /// </summary>
    public Vector3 FarBottomLeft;

    /// <summary>
    /// Gets or sets the bottom-right corner of the far face.
    /// </summary>
    public Vector3 FarBottomRight;
}
