using System.Numerics;

namespace Imago.Graphics.Textures;

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

public static class TextureRegionExtensions
{
    /// <summary>
    /// Gets a vector containing the size and offset of the packed texture in texture space coordinates.
    /// The first two components are the size of the texture
    /// and the last two components are the offset of the texture in the base texture.
    /// </summary>
    /// <returns>A vector containing the size and offset of the packed texture in texture space coordinates.</returns>
    public static Vector4 GetTextureST(this ITextureRegion region)
    {
        Vector2 size = region.BottomRight - region.TopLeft;
        return new Vector4(size.X, size.Y, region.TopLeft.X, region.TopLeft.Y);
    }
}
