using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Engine.Rendering;
using LifeSim.Support;
using Veldrid;

namespace LifeSim.Engine.Meshes;

[StructLayout(LayoutKind.Sequential)]
public struct SkinnedVertex
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TexCoords;
    public Vector4UShort Joints;
    public Vector4 Weights;

    public SkinnedVertex(Vector3 position, Vector3 normal, Vector2 uv, Vector4UShort joints, Vector4 weights)
    {
        this.Position = position;
        this.Normal = normal;
        this.TexCoords = uv;
        this.Joints = joints;
        this.Weights = weights;
    }

    private static VertexFormat? _vertexFormat;

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
