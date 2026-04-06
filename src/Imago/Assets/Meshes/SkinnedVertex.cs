using System.Numerics;
using System.Runtime.InteropServices;
using Imago.Support.Numerics;
using NeoVeldrid;

namespace Imago.Assets.Meshes;

/// <summary>
/// Represents a vertex with skinning information for animated meshes, including bone joints and weights.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct SkinnedVertex
{
    /// <summary>
    /// Gets or sets the position of the vertex in 3D space.
    /// </summary>
    public Vector3 Position;
    /// <summary>
    /// Gets or sets the normal vector of the vertex.
    /// </summary>
    public Vector3 Normal;
    /// <summary>
    /// Gets or sets the texture coordinates of the vertex.
    /// </summary>
    public Vector2 TexCoords;
    /// <summary>
    /// Gets or sets the bone joint indices that influence this vertex.
    /// </summary>
    public Vector4UShort Joints;
    /// <summary>
    /// Gets or sets the weights for each bone joint that influences this vertex.
    /// </summary>
    public Vector4 Weights;

    /// <summary>
    /// Initializes a new instance of the <see cref="SkinnedVertex"/> struct.
    /// </summary>
    /// <param name="position">The position of the vertex.</param>
    /// <param name="normal">The normal vector of the vertex.</param>
    /// <param name="uv">The texture coordinates of the vertex.</param>
    /// <param name="joints">The bone joint indices that influence this vertex.</param>
    /// <param name="weights">The weights for each bone joint.</param>
    public SkinnedVertex(Vector3 position, Vector3 normal, Vector2 uv, Vector4UShort joints, Vector4 weights)
    {
        this.Position = position;
        this.Normal = normal;
        this.TexCoords = uv;
        this.Joints = joints;
        this.Weights = weights;
    }

    private static VertexFormat? _vertexFormat;

    /// <summary>
    /// Gets the vertex format description for skinned vertices.
    /// </summary>
    public static VertexFormat VertexFormat
    {
        get
        {
            if (_vertexFormat != null) return _vertexFormat;

            _vertexFormat = new VertexFormat("SkinnedVertex", new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Joints", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UShort4),
                new VertexElementDescription("Weights", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
            ));
            _vertexFormat.IsSkinned = true;
            return _vertexFormat;
        }
    }
}
