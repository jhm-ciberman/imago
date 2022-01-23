using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Engine.Rendering;
using Veldrid;

namespace LifeSim.Engine.Gltf;

public class SkinnedMeshData : BasicMeshData
{
    public Vector4UShort[] Joints { get; set; }

    public Vector4[] Weights { get; set; }

    public override VertexFormat VertexFormat => SkinnedVertex.VertexFormat;

    public SkinnedMeshData(ushort[] indices, Vector3[] positions, Vector3[]? normals, Vector2[]? uvs, Vector4UShort[] joints, Vector4[] weights)
        : base(indices, positions, normals, uvs)
    {
        this.Joints = joints;
        this.Weights = weights;
    }

    public override DeviceBuffer CreateVertexBuffer(GraphicsDevice gd)
    {
        this.Validate();

        SkinnedVertex[] vertices = ArrayPool<SkinnedVertex>.Shared.Rent(this.Positions.Length);
        for (var i = 0; i < this.Positions.Length; i++)
        {
            vertices[i].Position = this.Positions[i];
            vertices[i].Normal = this.Normals[i];
            vertices[i].TexCoords = this.TexCoords[i];
            vertices[i].Joints = this.Joints[i];
            vertices[i].Weights = this.Weights[i];
        }

        uint sizeInBytes = (uint) (Marshal.SizeOf<SkinnedVertex>() * this.Positions.Length);

        DeviceBuffer vertexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(sizeInBytes, BufferUsage.VertexBuffer));
        gd.UpdateBuffer(vertexBuffer, 0, new ReadOnlySpan<SkinnedVertex>(vertices, 0, this.Positions.Length));
        ArrayPool<SkinnedVertex>.Shared.Return(vertices);
        return vertexBuffer;
    }
}