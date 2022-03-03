using System.Numerics;
using LifeSim.Engine.Rendering;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine;

/// <summary>
/// An object describing generic mesh data. This can be used to construct a <see cref="VertexBuffer"/> and
/// <see cref="IndexBuffer"/>.
/// </summary>
public interface IMeshData
{
    /// <summary>
    /// Gets or sets the vertex positions.
    /// </summary>
    Vector3[] Positions { get; }

    /// <summary>
    /// Gets or sets the vertex indices.
    /// </summary>
    ushort[] Indices { get; }

    /// <summary>
    /// Gets the vertex format of the mesh.
    /// </summary>
    VertexFormat VertexFormat { get; }

    /// <summary>
    /// Constructs a <see cref="VertexBuffer"/> from this <see cref="MeshData"/>.
    /// </summary>
    /// <param name="factory">The <see cref="ResourceFactory"/> to use for device resource creation.</param>
    /// <param name="cl">The <see cref="CommandList"/> to use for device resource creation.</param>
    /// <returns></returns>
    DeviceBuffer CreateVertexBuffer(GraphicsDevice gd);

    /// <summary>
    /// Constructs a <see cref="IndexBuffer"/> from this <see cref="MeshData"/>.
    /// </summary>
    /// <param name="factory">The <see cref="ResourceFactory"/> to use for device resource creation.</param>
    /// <param name="cl">The <see cref="CommandList"/> to use for device resource creation.</param>
    /// <returns></returns>
    DeviceBuffer CreateIndexBuffer(GraphicsDevice gd);

}