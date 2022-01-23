using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Engine.Rendering;
using Veldrid;

namespace LifeSim.Engine;

[StructLayout(LayoutKind.Sequential)]
public struct BasicVertex
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TexCoord;

    public BasicVertex(Vector3 position, Vector3 normal, Vector2 uv)
    {
        this.Position = position;
        this.Normal = normal;
        this.TexCoord = uv;
    }

    public BasicVertex(Vector3 position, Vector2 uv)
    {
        this.Position = position;
        this.Normal = Vector3.Zero;
        this.TexCoord = uv;
    }

    private static VertexFormat? _vertexFormat;

    public static VertexFormat VertexFormat
    {
        get
        {
            if (_vertexFormat != null) return _vertexFormat;

            _vertexFormat = new VertexFormat(new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
            ));
            _vertexFormat.IsSurface = true;
            return _vertexFormat;
        }
    }
}