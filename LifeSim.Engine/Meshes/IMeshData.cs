using System.Numerics;
using LifeSim.Engine.Rendering;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Meshes;

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
    /// Constructs a <see cref="DeviceBuffer"/> to use as vertex buffer from this <see cref="MeshData"/>.
    /// </summary>
    /// <param name="gd">The <see cref="GraphicsDevice"/> to use for device resource creation.</param>
    /// <returns>The constructed <see cref="DeviceBuffer"/>.</returns>
    DeviceBuffer CreateVertexBuffer(GraphicsDevice gd);

    /// <summary>
    /// Constructs a <see cref="DeviceBuffer"/> to use as undex buffer from this <see cref="MeshData"/>.
    /// </summary>
    /// <param name="gd">The <see cref="GraphicsDevice"/> to use for device resource creation.</param>
    /// <returns>The constructed <see cref="DeviceBuffer"/>.</returns>
    DeviceBuffer CreateIndexBuffer(GraphicsDevice gd);
}
