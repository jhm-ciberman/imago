using System;
using System.Numerics;
using Imago.Assets.Textures;
using Imago.Support.Numerics;

namespace Imago.Assets.TexturePacking;

/// <summary>
/// Represents a region within a larger texture atlas, defining its location and size in UV coordinates.
/// </summary>
public class PackedTexture : ITextureRegion
{
    /// <summary>
    /// Gets the underlying texture atlas that this packed texture belongs to.
    /// </summary>
    public Texture Texture { get; }

    /// <summary>
    /// Gets the UV coordinate of the top-left corner of the packed region within the texture atlas.
    /// </summary>
    public Vector2 TopLeft { get; }

    /// <summary>
    /// Gets the UV coordinate of the bottom-right corner of the packed region within the texture atlas.
    /// </summary>
    public Vector2 BottomRight { get; }

    /// <summary>
    /// Gets the size of the packed region in UV coordinates.
    /// </summary>
    public Vector2 Size => this.BottomRight - this.TopLeft;

    /// <summary>
    /// Initializes a new instance of the <see cref="PackedTexture"/> class.
    /// </summary>
    /// <param name="texture">The texture atlas.</param>
    /// <param name="topLeft">The top-left UV coordinate of the region.</param>
    /// <param name="bottomRight">The bottom-right UV coordinate of the region.</param>
    public PackedTexture(Texture texture, Vector2 topLeft, Vector2 bottomRight)
    {
        this.Texture = texture;
        this.TopLeft = topLeft;
        this.BottomRight = bottomRight;
    }

    /// <summary>
    /// Gets the size of the packed texture region in pixels.
    /// </summary>
    public Vector2Int PixelSize => new Vector2Int(
        (int)Math.Round(this.Size.X * this.Texture.Size.X),
        (int)Math.Round(this.Size.Y * this.Texture.Size.Y)
    );

    /// <summary>
    /// Gets the top-left position of the packed texture region in pixels.
    /// </summary>
    public Vector2Int PixelTopLeft => new Vector2Int(
        (int)Math.Round(this.TopLeft.X * this.Texture.Size.X),
        (int)Math.Round(this.TopLeft.Y * this.Texture.Size.Y)
    );

    /// <summary>
    /// Gets the bottom-right position of the packed texture region in pixels.
    /// </summary>
    public Vector2Int PixelBottomRight => new Vector2Int(
        (int)Math.Round(this.BottomRight.X * this.Texture.Size.X),
        (int)Math.Round(this.BottomRight.Y * this.Texture.Size.Y)
    );

    /// <summary>
    /// Gets a vector containing the UV scale (XY) and offset (ZW) for this packed texture.
    /// This is useful for passing texture coordinate transformations to a shader.
    /// </summary>
    /// <returns>A <see cref="Vector4"/> where XY is the size and ZW is the top-left offset.</returns>
    public Vector4 GetTextureST()
    {
        Vector2 size = this.Size;
        return new Vector4(size.X, size.Y, this.TopLeft.X, this.TopLeft.Y);
    }

    /// <summary>
    /// Returns a string representation of the packed texture.
    /// </summary>
    /// <returns>A string representation of the packed texture.</returns>
    public override string ToString()
    {
        return $"{this.Texture.Name} {this.TopLeft} {this.BottomRight}";
    }

    /// <summary>
    /// Creates a new <see cref="PackedTexture"/> representing a sub-region of this packed texture.
    /// </summary>
    /// <param name="topLeft">The top-left UV coordinate of the sub-region, relative to this packed texture.</param>
    /// <param name="bottomRight">The bottom-right UV coordinate of the sub-region, relative to this packed texture.</param>
    /// <returns>A new <see cref="PackedTexture"/> for the specified sub-region.</returns>
    public PackedTexture SubTexture(Vector2 topLeft, Vector2 bottomRight)
    {
        return new PackedTexture(this.Texture, this.TopLeft + topLeft * this.Size, this.TopLeft + bottomRight * this.Size);
    }
}
