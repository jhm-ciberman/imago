using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace LifeSim.Imago.Assets.Meshes;

/// <summary>
/// Represents mesh data for terrain, extending <see cref="BasicMeshData"/> with light information.
/// </summary>
public class TerrainMeshData : BasicMeshData
{
    /// <summary>
    /// Gets or sets the array of 2D lightmap coordinates for each vertex.
    /// </summary>
    public Vector2[] Lights { get; set; }

    /// <inheritdoc/>
    public override VertexFormat VertexFormat => TerrainVertex.VertexFormat;

    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainMeshData"/> class.
    /// </summary>
    /// <param name="indices">The array of indices defining the triangles.</param>
    /// <param name="positions">The array of vertex positions.</param>
    /// <param name="normals">Optional: The array of normal vectors. If null, normals will be recomputed.</param>
    /// <param name="texCoords">The array of texture coordinates.</param>
    /// <param name="lights">The array of lightmap coordinates.</param>
    public TerrainMeshData(ushort[] indices, Vector3[] positions, Vector3[]? normals, Vector2[] texCoords, Vector2[] lights)
        : base(indices, positions, normals, texCoords)
    {
        this.Lights = lights;
    }

    /// <inheritdoc/>
    protected override void Validate()
    {
        base.Validate();

        if (this.Lights.Length != this.Positions.Length)
            throw new ArgumentException("The number of lights vertices must match the number of positions.");
    }

    /// <inheritdoc/>
    public override DeviceBuffer CreateVertexBuffer(GraphicsDevice gd)
    {
        this.Validate();

        TerrainVertex[] vertices = ArrayPool<TerrainVertex>.Shared.Rent(this.Positions.Length);
        for (var i = 0; i < this.Positions.Length; i++)
        {
            vertices[i].Position = this.Positions[i];
            vertices[i].Normal = this.Normals[i];
            vertices[i].TexCoords = this.TexCoords[i];
            vertices[i].Light = this.Lights[i];
        }

        uint sizeInBytes = (uint)(this.Positions.Length * Unsafe.SizeOf<TerrainVertex>());
        DeviceBuffer vertexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(sizeInBytes, BufferUsage.VertexBuffer));
        gd.UpdateBuffer(vertexBuffer, 0, new ReadOnlySpan<TerrainVertex>(vertices, 0, this.Positions.Length));

        ArrayPool<TerrainVertex>.Shared.Return(vertices);
        return vertexBuffer;
    }
}
