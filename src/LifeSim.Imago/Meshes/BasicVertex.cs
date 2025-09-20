using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace LifeSim.Imago.Meshes;

/// <summary>
/// Represents a basic vertex with position, normal, and texture coordinates.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct BasicVertex
{
    /// <summary>
    /// Gets or sets the 3D position of the vertex.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// Gets or sets the normal vector of the vertex.
    /// </summary>
    public Vector3 Normal;

    /// <summary>
    /// Gets or sets the texture coordinates of the vertex.
    /// </summary>
    public Vector2 TexCoord;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicVertex"/> struct.
    /// </summary>
    /// <param name="position">The 3D position of the vertex.</param>
    /// <param name="normal">The normal vector of the vertex.</param>
    /// <param name="uv">The texture coordinates of the vertex.</param>
    public BasicVertex(Vector3 position, Vector3 normal, Vector2 uv)
    {
        this.Position = position;
        this.Normal = normal;
        this.TexCoord = uv;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicVertex"/> struct with zero normal.
    /// </summary>
    /// <param name="position">The 3D position of the vertex.</param>
    /// <param name="uv">The texture coordinates of the vertex.</param>
    public BasicVertex(Vector3 position, Vector2 uv)
    {
        this.Position = position;
        this.Normal = Vector3.Zero;
        this.TexCoord = uv;
    }

    private static VertexFormat? _vertexFormat;

    /// <summary>
    /// Gets the vertex format descriptor for this vertex type.
    /// </summary>
    public static VertexFormat VertexFormat
    {
        get
        {
            if (_vertexFormat != null) return _vertexFormat;

            _vertexFormat = new VertexFormat("BasicVertex", new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
            ));
            return _vertexFormat;
        }
    }
}
