using System.Numerics;
using LifeSim.Imago.TexturePacking;

namespace LifeSim.Imago.Textures;

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

/// <summary>
/// Provides extension methods for texture region operations and calculations.
/// </summary>
public static class TextureRegionExtensions
{
    /// <summary>
    /// Gets a vector containing the size and offset of the packed texture in texture space coordinates.
    /// The first two components are the size of the texture
    /// and the last two components are the offset of the texture in the base texture.
    /// </summary>
    /// <param name="region">The packed texture.</param>
    /// <returns>A vector containing the size and offset of the packed texture in texture space coordinates.</returns>
    public static Vector4 GetTextureST(this ITextureRegion region)
    {
        Vector2 size = region.BottomRight - region.TopLeft;
        return new Vector4(size.X, size.Y, region.TopLeft.X, region.TopLeft.Y);
    }

    /// <summary>
    /// Gets a sub texture of this packed texture.
    /// </summary>
    /// <param name="region">The packed texture.</param>
    /// <param name="topLeft">The top left corner of the sub texture in texture space coordinates.</param>
    /// <param name="bottomRight">The bottom right corner of the sub texture in texture space coordinates.</param>
    /// <returns></returns>
    public static ITextureRegion SubTexture(this ITextureRegion region, Vector2 topLeft, Vector2 bottomRight)
    {
        var tl = Vector2.Lerp(region.TopLeft, region.BottomRight, topLeft);
        var br = Vector2.Lerp(region.TopLeft, region.BottomRight, bottomRight);

        return new PackedTexture(region.Texture, tl, br);
    }

    /// <summary>
    /// Creates an array of texture regions from this texture that can be used as a flip book.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <param name="frameCount">The number of frames in the flip book.</param>
    /// <returns></returns>
    public static ITextureRegion[] MakeFlipBook(this ITextureRegion texture, int frameCount)
    {
        if (frameCount == 1) return [texture];

        var regions = new ITextureRegion[frameCount];
        var u = 1f / frameCount;
        for (int i = 0; i < frameCount; i++)
        {
            var tl = new Vector2(i * u, 0);
            var br = new Vector2((i + 1) * u, 1);
            regions[i] = texture.SubTexture(tl, br);
        }

        return regions;
    }

    /// <summary>
    /// Creates a texture region that is mirrored horizontally.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <returns>The mirrored texture region.</returns>
    public static ITextureRegion MirrorX(this ITextureRegion texture)
    {
        return texture.SubTexture(new Vector2(1f, 0f), new Vector2(0f, 1f));
    }

    /// <summary>
    /// Creates a texture region that is mirrored vertically.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <returns>The mirrored texture region.</returns>
    public static ITextureRegion MirrorY(this ITextureRegion texture)
    {
        return texture.SubTexture(new Vector2(0f, 1f), new Vector2(1f, 0f));
    }

    /// <summary>
    /// Transform the given UV coordinates to the texture space coordinates.
    /// </summary>
    /// <param name="region">The texture region.</param>
    /// <param name="uv">The UV coordinates.</param>
    /// <returns>The transformed texture space coordinates.</returns>
    public static Vector2 TransformUV(this ITextureRegion region, Vector2 uv)
    {
        var size = region.BottomRight - region.TopLeft;
        var offset = region.TopLeft;
        return uv * size + offset;
    }

    /// <summary>
    /// Gets the size of the texture region in pixels.
    /// </summary>
    /// <param name="region">The texture region.</param>
    /// <returns>The size of the texture region in pixels.</returns>
    public static Vector2 GetPixelSize(this ITextureRegion region)
    {
        if (region is PackedTexture packed)
        {
            return packed.PixelSize;
        }

        var size = (region.BottomRight - region.TopLeft) * region.Texture.Size;
        return new Vector2(size.X, size.Y);
    }
}
