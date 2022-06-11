using System;
using System.Numerics;
using LifeSim.Engine.Rendering;
using Veldrid;

namespace LifeSim.Engine.Meshes;

public abstract class BaseMeshData : IMeshData
{
    public ushort[] Indices { get; set; }

    public Vector3[] Positions { get; set; }

    public BaseMeshData(ushort[] indices, Vector3[] vertices)
    {
        this.Indices = indices;
        this.Positions = vertices;

        if (this.Indices.Length % 3 != 0)
        {
            throw new ArgumentException("The number of indices must be a multiple of 3.");
        }
    }

    public abstract VertexFormat VertexFormat { get; }

    public void FlipIndices()
    {
        for (var i = 0; i < this.Indices.Length; i += 3)
        {
            var a = this.Indices[i + 0];
            var b = this.Indices[i + 1];
            var c = this.Indices[i + 2];

            this.Indices[i + 0] = c;
            this.Indices[i + 1] = b;
            this.Indices[i + 2] = a;
        }
    }

    public void Translate(Vector3 translation)
    {
        for (var i = 0; i < this.Positions.Length; i++)
        {
            this.Positions[i] += translation;
        }
    }

    protected virtual void Validate()
    {
        if (this.Indices.Length % 3 != 0)
        {
            throw new ArgumentException("The number of indices must be a multiple of 3.");
        }
    }

    /// <inheritdoc />
    public abstract DeviceBuffer CreateVertexBuffer(GraphicsDevice gd);

    /// <inheritdoc />
    public DeviceBuffer CreateIndexBuffer(GraphicsDevice gd)
    {
        DeviceBuffer indexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint) (sizeof(ushort) * this.Indices.Length), BufferUsage.IndexBuffer));
        gd.UpdateBuffer<ushort>(indexBuffer, 0, this.Indices);
        return indexBuffer;
    }
}