using System.Numerics;

namespace Imago.Rendering;

/// <summary>
/// Represents a texure region in a texture atlas.
/// </summary>
public interface ITextureRegion
{
    /// <summary>
    /// Gets the texture atlas in which the region is located.
    /// </summary>
    public Texture Texture { get; }

    /// <summary>
    /// Gets the top left coordinate of the region in texture UV space.
    /// </summary>
    public Vector2 TopLeft { get; }

    /// <summary>
    /// Gets the bottom right coordinate of the region in texture UV space.
    /// </summary>
    public Vector2 BottomRight { get; }
}
