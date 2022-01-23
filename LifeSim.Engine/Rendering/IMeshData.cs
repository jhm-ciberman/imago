using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering;

/// <summary>
/// An object describing generic mesh data. This can be used to construct a <see cref="VertexBuffer"/> and
/// <see cref="IndexBuffer"/>, and also exposes functionality for bounding box and sphere computation.
/// </summary>
public interface IMeshData
{
    /// <summary>
    /// Gets or sets the vertex positions.
    /// </summary>
    Vector3[] Positions { get; set; }

    /// <summary>
    /// Gets or sets the vertex indices.
    /// </summary>
    ushort[] Indices { get; set; }

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