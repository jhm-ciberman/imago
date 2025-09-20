using System.Numerics;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.Rendering.Sprites;

/// <summary>
/// Represents a vertex used for sprite rendering with position, texture coordinates, and color information.
/// </summary>
public struct SpriteVertex
{
    /// <summary>
    /// Gets or sets the 3D position of the vertex.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// Gets or sets the texture coordinates (UV) of the vertex.
    /// </summary>
    public Vector2 Uv;

    /// <summary>
    /// Gets or sets the packed color value of the vertex.
    /// </summary>
    public uint Color;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteVertex"/> struct.
    /// </summary>
    /// <param name="position">The 3D position of the vertex.</param>
    /// <param name="uv">The texture coordinates of the vertex.</param>
    /// <param name="color">The color of the vertex.</param>
    public SpriteVertex(Vector3 position, Vector2 uv, Color color)
    {
        this.Position = position;
        this.Uv = uv;
        this.Color = color.ToPackedUInt();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteVertex"/> struct.
    /// </summary>
    /// <param name="position">The 3D position of the vertex.</param>
    /// <param name="uv">The texture coordinates of the vertex.</param>
    /// <param name="color">The packed color value of the vertex.</param>
    public SpriteVertex(Vector3 position, Vector2 uv, uint color)
    {
        this.Position = position;
        this.Uv = uv;
        this.Color = color;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteVertex"/> struct.
    /// </summary>
    /// <param name="position">The 2D position of the vertex.</param>
    /// <param name="depth">The depth (Z coordinate) of the vertex.</param>
    /// <param name="uv">The texture coordinates of the vertex.</param>
    /// <param name="color">The color of the vertex.</param>
    public SpriteVertex(Vector2 position, float depth, Vector2 uv, Color color)
    {
        this.Position = new Vector3(position, depth);
        this.Uv = uv;
        this.Color = color.ToPackedUInt();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteVertex"/> struct.
    /// </summary>
    /// <param name="x">The X coordinate of the vertex.</param>
    /// <param name="y">The Y coordinate of the vertex.</param>
    /// <param name="z">The Z coordinate of the vertex.</param>
    /// <param name="u">The U texture coordinate of the vertex.</param>
    /// <param name="v">The V texture coordinate of the vertex.</param>
    /// <param name="color">The color of the vertex.</param>
    public SpriteVertex(float x, float y, float z, float u, float v, Color color)
    {
        this.Position = new Vector3(x, y, z);
        this.Uv = new Vector2(u, v);
        this.Color = color.ToPackedUInt();
    }
}
