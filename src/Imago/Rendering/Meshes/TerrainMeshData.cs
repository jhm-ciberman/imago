using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using Imago.Rendering;
using Veldrid;

namespace Imago.Rendering.Meshes;

public class TerrainMeshData : BasicMeshData
{
    public Vector2[] Lights { get; set; }

    public override VertexFormat VertexFormat => TerrainVertex.VertexFormat;

    public TerrainMeshData(ushort[] indices, Vector3[] positions, Vector3[]? normals, Vector2[] texCoords, Vector2[] lights)
        : base(indices, positions, normals, texCoords)
    {
        this.Lights = lights;
    }

    protected override void Validate()
    {
        base.Validate();

        if (this.Lights.Length != this.Positions.Length)
            throw new ArgumentException("The number of lights vertices must match the number of positions.");
    }

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
