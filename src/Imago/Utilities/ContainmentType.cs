namespace Imago.Utilities;

/// <summary>
/// Defines the possible relationships between two geometric shapes or volumes.
/// </summary>
public enum ContainmentType
{
    /// <summary>
    /// The shapes do not intersect or touch each other.
    /// </summary>
    Disjoint,

    /// <summary>
    /// One shape completely contains the other.
    /// </summary>
    Contains,

    /// <summary>
    /// The shapes partially overlap or intersect.
    /// </summary>
    Intersects,
}
