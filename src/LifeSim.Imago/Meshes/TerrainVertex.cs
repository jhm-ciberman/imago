using System.Numerics;
using Veldrid;

namespace LifeSim.Imago.Meshes;

/// <summary>
/// Represents a vertex used for terrain rendering with position, normal, texture coordinates, and lighting information.
/// </summary>
public struct TerrainVertex
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
    /// Gets or sets the lighting information for the vertex.
    /// </summary>
    public Vector2 Light;

    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainVertex"/> struct.
    /// </summary>
    /// <param name="pos">The position of the vertex.</param>
    /// <param name="uv">The texture coordinates of the vertex.</param>
    /// <param name="light">The lighting information for the vertex.</param>
    public TerrainVertex(Vector3 pos, Vector2 uv, Vector2 light)
    {
        this.Position = pos;
        this.Normal = Vector3.Zero;
        this.TexCoords = uv;
        this.Light = light;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainVertex"/> struct with default lighting.
    /// </summary>
    /// <param name="pos">The position of the vertex.</param>
    /// <param name="uv">The texture coordinates of the vertex.</param>
    public TerrainVertex(Vector3 pos, Vector2 uv)
    {
        this.Position = pos;
        this.Normal = Vector3.Zero;
        this.TexCoords = uv;
        this.Light = Vector2.One;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainVertex"/> struct with individual coordinate values.
    /// </summary>
    /// <param name="x">The X coordinate of the vertex position.</param>
    /// <param name="y">The Y coordinate of the vertex position.</param>
    /// <param name="z">The Z coordinate of the vertex position.</param>
    /// <param name="u">The U texture coordinate.</param>
    /// <param name="v">The V texture coordinate.</param>
    public TerrainVertex(float x, float y, float z, float u, float v)
        : this(new Vector3(x, y, z), new Vector2(u, v)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainVertex"/> struct with individual coordinate values and lighting.
    /// </summary>
    /// <param name="x">The X coordinate of the vertex position.</param>
    /// <param name="y">The Y coordinate of the vertex position.</param>
    /// <param name="z">The Z coordinate of the vertex position.</param>
    /// <param name="u">The U texture coordinate.</param>
    /// <param name="v">The V texture coordinate.</param>
    /// <param name="light">The lighting information for the vertex.</param>
    public TerrainVertex(float x, float y, float z, float u, float v, Vector2 light)
        : this(new Vector3(x, y, z), new Vector2(u, v), light) { }

    /// <summary>
    /// Gets the X coordinate of the vertex position.
    /// </summary>
    public float X => this.Position.X;
    /// <summary>
    /// Gets the Y coordinate of the vertex position.
    /// </summary>
    public float Y => this.Position.Y;
    /// <summary>
    /// Gets the Z coordinate of the vertex position.
    /// </summary>
    public float Z => this.Position.Z;
    /// <summary>
    /// Gets the U texture coordinate.
    /// </summary>
    public float U => this.TexCoords.X;
    /// <summary>
    /// Gets the V texture coordinate.
    /// </summary>
    public float V => this.TexCoords.Y;

    /// <summary>
    /// Creates a new terrain vertex with full sunlight.
    /// </summary>
    /// <returns>A new <see cref="TerrainVertex"/> with full sunlight.</returns>
    public TerrainVertex WithSunlight() => new TerrainVertex(this.Position, this.TexCoords, Vector2.One);

    /// <summary>
    /// Linearly interpolates between two terrain vertices.
    /// </summary>
    /// <param name="a">The first vertex.</param>
    /// <param name="b">The second vertex.</param>
    /// <param name="t">The interpolation factor.</param>
    /// <returns>The interpolated vertex.</returns>
    public static TerrainVertex Lerp(TerrainVertex a, TerrainVertex b, float t)
    {
        Vector3 pos = Vector3.Lerp(a.Position, b.Position, t);
        Vector2 uv = Vector2.Lerp(a.TexCoords, b.TexCoords, t);
        Vector2 light = Vector2.Lerp(a.Light, b.Light, t);
        return new TerrainVertex(pos, uv, light);
    }

    /// <summary>
    /// Creates a new terrain vertex translated by the specified vector.
    /// </summary>
    /// <param name="v">The translation vector.</param>
    /// <returns>A new <see cref="TerrainVertex"/> with the translated position.</returns>
    public TerrainVertex Translate(Vector3 v)
    {
        return new TerrainVertex(this.Position + v, this.TexCoords, this.Light);
    }

    /// <summary>
    /// Creates a new terrain vertex translated by the specified vector with full sunlight.
    /// </summary>
    /// <param name="v">The translation vector.</param>
    /// <returns>A new <see cref="TerrainVertex"/> with the translated position and full sunlight.</returns>
    public TerrainVertex TranslateWithSunlight(Vector3 v)
    {
        return new TerrainVertex(this.Position + v, this.TexCoords, Vector2.One);
    }

    private static VertexFormat? _vertexFormat;
    /// <summary>
    /// Gets the vertex format description for terrain vertices.
    /// </summary>
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
            return _vertexFormat;
        }
    }
}
