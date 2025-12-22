using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using LifeSim.Support.Numerics;
using Veldrid;

namespace LifeSim.Imago.Assets.Meshes;

/// <summary>
/// Represents mesh data for a skinned 3D model, extending <see cref="BasicMeshData"/> with joint and weight information.
/// </summary>
public class SkinnedMeshData : BasicMeshData
{
    /// <summary>
    /// Gets or sets the array of joint indices for each vertex.
    /// Each <see cref="Vector4UShort"/> contains up to 4 joint indices.
    /// </summary>
    public Vector4UShort[] Joints { get; set; }

    /// <summary>
    /// Gets or sets the array of joint weights for each vertex.
    /// Each <see cref="Vector4"/> contains up to 4 weights, corresponding to the <see cref="Joints"/>.
    /// </summary>
    public Vector4[] Weights { get; set; }

    /// <inheritdoc/>
    public override VertexFormat VertexFormat => SkinnedVertex.VertexFormat;

    /// <summary>
    /// Initializes a new instance of the <see cref="SkinnedMeshData"/> class.
    /// </summary>
    /// <param name="indices">The array of indices defining the triangles.</param>
    /// <param name="positions">The array of vertex positions.</param>
    /// <param name="normals">Optional: The array of normal vectors. If null, normals will be recomputed.</param>
    /// <param name="uvs">Optional: The array of texture coordinates. If null, default zero vectors will be used.</param>
    /// <param name="joints">The array of joint indices for each vertex.</param>
    /// <param name="weights">The array of joint weights for each vertex.</param>
    public SkinnedMeshData(ushort[] indices, Vector3[] positions, Vector3[]? normals, Vector2[]? uvs, Vector4UShort[] joints, Vector4[] weights)
        : base(indices, positions, normals, uvs)
    {
        this.Joints = joints;
        this.Weights = weights;
    }

    /// <inheritdoc/>
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

        uint sizeInBytes = (uint)(this.Positions.Length * Unsafe.SizeOf<SkinnedVertex>());
        DeviceBuffer vertexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(sizeInBytes, BufferUsage.VertexBuffer));
        gd.UpdateBuffer(vertexBuffer, 0, new ReadOnlySpan<SkinnedVertex>(vertices, 0, this.Positions.Length));

        ArrayPool<SkinnedVertex>.Shared.Return(vertices);
        return vertexBuffer;
    }
}
