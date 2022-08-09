using System;
using System.Numerics;
using LifeSim.Engine.Rendering;
using Veldrid;

namespace LifeSim.Engine.Meshes;

public struct ChunkVertex
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TexCoords;
    public Vector2 Light;

    public ChunkVertex(Vector3 pos, Vector2 uv, Vector2 light)
    {
        this.Position = pos;
        this.Normal = Vector3.Zero;
        this.TexCoords = uv;
        this.Light = light;
    }

    public ChunkVertex(Vector3 pos, Vector2 uv)
    {
        this.Position = pos;
        this.Normal = Vector3.Zero;
        this.TexCoords = uv;
        this.Light = Vector2.One;
    }

    public ChunkVertex(float x, float y, float z, float u, float v)
        : this(new Vector3(x, y, z), new Vector2(u, v)) { }

    public ChunkVertex(float x, float y, float z, float u, float v, Vector2 light)
        : this(new Vector3(x, y, z), new Vector2(u, v), light) { }

    public float X => this.Position.X;
    public float Y => this.Position.Y;
    public float Z => this.Position.Z;
    public float U => this.TexCoords.X;
    public float V => this.TexCoords.Y;

    public ChunkVertex WithSunlight() => new ChunkVertex(this.Position, this.TexCoords, Vector2.One);

    public static ChunkVertex Lerp(ChunkVertex a, ChunkVertex b, float t)
    {
        Vector3 pos = Vector3.Lerp(a.Position, b.Position, t);
        Vector2 uv = Vector2.Lerp(a.TexCoords, b.TexCoords, t);
        Vector2 light = Vector2.Lerp(a.Light, b.Light, t);
        return new ChunkVertex(pos, uv, light);
    }

    public ChunkVertex Translate(Vector3 v)
    {
        return new ChunkVertex(this.Position + v, this.TexCoords, this.Light);
    }

    public ChunkVertex TranslateWithSunlight(Vector3 v)
    {
        return new ChunkVertex(this.Position + v, this.TexCoords, Vector2.One);
    }

    private static VertexFormat? _vertexFormat;
    public static VertexFormat VertexFormat
    {
        get
        {
            if (_vertexFormat != null) return _vertexFormat;

            _vertexFormat = new VertexFormat("ChunkVertex", new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Light", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
            ));
            _vertexFormat.IsSurface = true;
            return _vertexFormat;
        }
    }
}