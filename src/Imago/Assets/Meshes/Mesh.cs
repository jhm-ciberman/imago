using System;
using CommunityToolkit.Diagnostics;
using Imago.Rendering;
using NeoVeldrid;
using BoundingBox = Imago.Utilities.BoundingBox;

namespace Imago.Assets.Meshes;

/// <summary>
/// Represents a collection of vertices and indices uploaded to the GPU, ready for rendering.
/// </summary>
public class Mesh : IDisposable
{
    private static int _count = 0;

    /// <summary>
    /// Gets the unique identifier for this mesh.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Gets the number of indices in the mesh.
    /// </summary>
    public uint IndexCount { get; private set; }

    /// <summary>
    /// Gets the vertex format of the mesh.
    /// </summary>
    public VertexFormat VertexFormat { get; private set; }

    /// <summary>
    /// Gets the NeoVeldrid device buffer containing the vertex data.
    /// </summary>
    public DeviceBuffer VeldridVertexBuffer { get; private set; }

    /// <summary>
    /// Gets the NeoVeldrid device buffer containing the index data.
    /// </summary>
    public DeviceBuffer VeldridIndexBuffer { get; private set; }

    /// <summary>
    /// Gets the axis-aligned bounding box that contains all vertices of the mesh.
    /// </summary>
    public BoundingBox BoundingBox { get; private set; }

    /// <summary>
    /// Gets the original <see cref="MeshData"/> used to create this mesh.
    /// </summary>
    public MeshData MeshData { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this mesh has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mesh"/> class.
    /// </summary>
    /// <param name="meshData">The mesh data to upload to the GPU.</param>
    public Mesh(MeshData meshData)
    {
        Guard.IsNotEmpty(meshData.Indices);

        var gd = Renderer.Instance.GraphicsDevice;
        this.Id = ++_count;
        this.VertexFormat = meshData.VertexFormat;
        this.IndexCount = (uint)meshData.Indices.Length;
        this.BoundingBox = BoundingBox.CreateFromVertices(meshData.Positions);
        this.VeldridIndexBuffer = meshData.CreateIndexBuffer(gd);
        this.VeldridVertexBuffer = meshData.CreateVertexBuffer(gd);
        this.MeshData = meshData;

        Renderer.Instance.RegisterDisposable(this);
    }

    /// <summary>
    /// Disposes the mesh and releases the underlying GPU resources.
    /// </summary>
    public void Dispose()
    {
        if (this.IsDisposed) return;
        this.IsDisposed = true;
        Renderer.Instance.DisposeWhenIdle(this.VeldridVertexBuffer);
        Renderer.Instance.DisposeWhenIdle(this.VeldridIndexBuffer);

        Renderer.Instance.UnregisterDisposable(this);
    }
}
