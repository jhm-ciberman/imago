using System.Numerics;
using LifeSim.Engine.Rendering;


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
    /// <param name="topLeft">The coordinate of the top left corner of the packed texture in the base texture.</param>
    /// <param name="bottomRight">The coordinate of the bottom right corner of the packed texture in the base texture.</param>
    public PackedTexture(Texture texture, Vector2 topLeft, Vector2 bottomRight)
    {
        this.Texture = texture;
        this.TopLeft = topLeft;
        this.BottomRight = bottomRight;
    }

    /// <summary>
    /// Gets the size of the packed texture in pixels.
    /// </summary>
    public Vector2 PixelSize => this.Size * this.Texture.Size;

    /// <summary>
    /// Gets the position of the left top corner of the packed texture in pixels.
    /// </summary>
    public Vector2 PixelTopLeft => this.TopLeft * this.Texture.Size;

    /// <summary>
    /// Gets the position of the right bottom corner of the packed texture in pixels.
    /// </summary>
    public Vector2 PixelBottomRight => this.BottomRight * this.Texture.Size;

    public Vector4 GetTextureST()
    {
        Vector2 size = this.Size;
        return new Vector4(size.X, size.Y, this.TopLeft.X, this.TopLeft.Y);
    }
}