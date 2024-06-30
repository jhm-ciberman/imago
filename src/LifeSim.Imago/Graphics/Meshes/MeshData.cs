using System;
using System.Numerics;
using Veldrid;

namespace LifeSim.Imago.Graphics.Meshes;

public abstract class MeshData
{
    /// <summary>
    /// Gets or sets the vertex indices.
    /// </summary>
    public ushort[] Indices { get; set; }

    /// <summary>
    /// Gets or sets the vertex positions.
    /// </summary>
    public Vector3[] Positions { get; set; }

    /// <summary>
    /// Gets the vertex format of the mesh.
    /// </summary>
    public abstract VertexFormat VertexFormat { get; }

    /// <summary>
    /// Constructs a new <see cref="MeshData"/> object.
    /// </summary>
    /// <param name="indices">The indices of the mesh.</param>
    /// <param name="vertices">The vertices of the mesh.</param>
    public MeshData(ushort[] indices, Vector3[] vertices)
    {
        this.Indices = indices;
        this.Positions = vertices;

        if (this.Indices.Length % 3 != 0)
            throw new ArgumentException("The number of indices must be a multiple of 3.");
    }

    /// <summary>
    /// Validates the mesh data.
    /// </summary>
    protected virtual void Validate()
    {
        if (this.Indices.Length % 3 != 0)
            throw new ArgumentException("The number of indices must be a multiple of 3.");
    }

    /// <summary>
    /// Constructs a <see cref="DeviceBuffer"/> to use as vertex buffer from this <see cref="MeshData"/>.
    /// </summary>
    /// <param name="gd">The <see cref="GraphicsDevice"/> to use for device resource creation.</param>
    /// <returns>The constructed <see cref="DeviceBuffer"/>.</returns>
    public abstract DeviceBuffer CreateVertexBuffer(GraphicsDevice gd);

    /// <summary>
    /// Constructs a <see cref="DeviceBuffer"/> to use as undex buffer from this <see cref="MeshData"/>.
    /// </summary>
    /// <param name="gd">The <see cref="GraphicsDevice"/> to use for device resource creation.</param>
    /// <returns>The constructed <see cref="DeviceBuffer"/>.</returns>
    public DeviceBuffer CreateIndexBuffer(GraphicsDevice gd)
    {
        DeviceBuffer indexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint) (sizeof(ushort) * this.Indices.Length), BufferUsage.IndexBuffer));
        gd.UpdateBuffer(indexBuffer, 0, this.Indices);
        return indexBuffer;
    }

    /// <summary>
    /// Flips the indices of the mesh so that the winding order is reversed.
    /// </summary>
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

    /// <summary>
    /// Translates the mesh by the specified translation vector.
    /// </summary>
    /// <param name="translation">The translation vector.</param>
    public void Translate(Vector3 translation)
    {
        for (var i = 0; i < this.Positions.Length; i++)
        {
            this.Positions[i] += translation;
        }
    }
}
