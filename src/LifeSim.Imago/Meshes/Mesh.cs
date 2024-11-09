using System;
using LifeSim.Imago.Rendering;
using Veldrid;
using BoundingBox = Veldrid.Utilities.BoundingBox;

namespace LifeSim.Imago.Meshes;

/// <summary>
/// Represents a mesh that can be rendered by the <see cref="Renderer"/>.
/// </summary>
public class Mesh : IDisposable
{
    private static int _count = 0;

    /// <summary>
    /// Gets the unique ID of the mesh.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Gets the number of indices in the mesh.
    /// </summary>
    public uint IndexCount { get; private set; }

    /// <summary>
    /// Gets the <see cref="VertexFormat"/> of the mesh.
    /// </summary>
    public VertexFormat VertexFormat { get; private set; }

    /// <summary>
    /// Gets the underlying Veldrid vertex buffer.
    /// </summary>
    public DeviceBuffer VeldridVertexBuffer { get; private set; }

    /// <summary>
    /// Gets the underlying Veldrid index buffer.
    /// </summary>
    public DeviceBuffer VeldridIndexBuffer { get; private set; }

    /// <summary>
    /// Gets the bounding box of the mesh.
    /// </summary>
    public BoundingBox BoundingBox { get; private set; }

    /// <summary>
    /// Gets the <see cref="MeshData"/> that was used to create the mesh.
    /// </summary>
    public MeshData MeshData { get; private set; }

    /// <summary>
    /// Gets whether the mesh is disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mesh"/> class.
    /// </summary>
    /// <param name="meshData">The <see cref="MeshData"/> that was used to create the mesh.</param>
    public Mesh(MeshData meshData)
    {
        if (meshData.Indices.Length == 0)
            throw new ArgumentException("The mesh data must contain at least one index.", nameof(meshData));

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
    /// Disposes the mesh.
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
