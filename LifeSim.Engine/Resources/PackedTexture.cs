using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Support;

namespace LifeSim.Engine.Resources;

/// <summary>
/// A packed texture is a texture that is packed into a single texture. The class
/// contains a reference to the base texture and the coordinates of the texture in
/// the base texture.
/// </summary>
public class PackedTexture
{
    /// <summary>
    /// Gets the base texture.
    /// </summary>
    public Texture Texture { get; }

    /// <summary>
    /// Gets the coordinate of the top left corner of the packed texture in texture space coordinates.
    /// </summary>    
    public Vector2 TopLeft { get; }

    /// <summary>
    /// Gets the coordinate of the bottom right corner of the packed texture in texture space coordinates.
    /// </summary>
    public Vector2 BottomRight { get; }

    /// <summary>
    /// Gets the size of the packed texture in texture space coordinates.
    /// </summary>
    public Vector2 Size => this.BottomRight - this.TopLeft;

    /// <summary>
    /// Creates a new packed texture.
    /// </summary>
    /// <param name="texture">The base texture.</param>
    /// <param name="topLeft">The coordinate of the top left corner of the packed texture in the base texture in texture space.</param>
    /// <param name="bottomRight">The coordinate of the bottom right corner of the packed texture in the base texture in texture space.</param>
    public PackedTexture(Texture texture, Vector2 topLeft, Vector2 bottomRight)
    {
        this.Texture = texture;
        this.TopLeft = topLeft;
        this.BottomRight = bottomRight;
    }

    /// <summary>
    /// Gets the size of the packed texture in pixels.
    /// </summary>
    public Vector2Int PixelSize => new Vector2Int((int)(this.Size.X * this.Texture.Size.X), (int)(this.Size.Y * this.Texture.Size.Y));

    /// <summary>
    /// Gets the position of the left top corner of the packed texture in pixels.
    /// </summary>
    public Vector2Int PixelTopLeft => new Vector2Int((int)(this.TopLeft.X * this.Texture.Size.X), (int)(this.TopLeft.Y * this.Texture.Size.Y));

    /// <summary>
    /// Gets the position of the right bottom corner of the packed texture in pixels.
    /// </summary>
    public Vector2Int PixelBottomRight => new Vector2Int((int)(this.BottomRight.X * this.Texture.Size.X), (int)(this.BottomRight.Y * this.Texture.Size.Y));

    /// <summary>
    /// Gets a vector containing the size and offset of the packed texture in texture space coordinates.
    /// The first two components are the size of the texture
    /// and the last two components are the offset of the texture in the base texture.
    /// </summary>
    /// <returns>A vector containing the size and offset of the packed texture in texture space coordinates.</returns>
    public Vector4 GetTextureST()
    {
        Vector2 size = this.Size;
        return new Vector4(size.X, size.Y, this.TopLeft.X, this.TopLeft.Y);
    }

    public override string ToString()
    {
        return $"{this.Texture.Name} {this.TopLeft} {this.BottomRight}";
    }
}
