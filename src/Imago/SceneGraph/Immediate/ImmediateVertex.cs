using System.Numerics;
using Imago.Support.Drawing;

namespace Imago.SceneGraph.Immediate;

/// <summary>
/// Represents a vertex used for immediate mode rendering.
/// </summary>
public struct ImmediateVertex
{
    /// <summary>
    /// Gets or sets the position of the vertex in 3D space.
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// Gets or sets the texture coordinates for the vertex.
    /// </summary>
    public Vector2 TextureCoords { get; set; }

    /// <summary>
    /// Gets or sets the color of the vertex as a packed uint value.
    /// </summary>
    public uint Color { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmediateVertex"/> struct.
    /// </summary>
    /// <param name="position">The position of the vertex in 3D space.</param>
    /// <param name="textureCoords">The texture coordinates for the vertex.</param>
    /// <param name="color">The color of the vertex.</param>
    public ImmediateVertex(Vector3 position, Vector2 textureCoords, Color color)
    {
        this.Position = position;
        this.TextureCoords = textureCoords;
        this.Color = color.ToPackedUInt();
    }
}
