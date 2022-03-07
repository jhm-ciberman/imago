using LifeSim.Engine.Rendering;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Resources;

public class Mesh : System.IDisposable
{
    private static int _count = 0;

    public int Id { get; private set; }
    public uint IndexCount { get; private set; }
    public VertexFormat VertexFormat { get; private set; }
    public DeviceBuffer VertexBuffer { get; private set; }
    public DeviceBuffer IndexBuffer { get; private set; }
    public BoundingBox BoundingBox { get; private set; }

    public IMeshData MeshData { get; private set; }

    public bool IsDisposed { get; private set; }

    public Mesh(IMeshData meshData)
    {
        var gd = Renderer.Instance.GraphicsDevice;
        this.Id = ++Mesh._count;
        this.VertexFormat = meshData.VertexFormat;
        this.IndexCount = (uint)meshData.Indices.Length;
        this.BoundingBox = BoundingBox.CreateFromVertices(meshData.Positions);
        this.IndexBuffer = meshData.CreateIndexBuffer(gd);
        this.VertexBuffer = meshData.CreateVertexBuffer(gd);
        this.MeshData = meshData;
    }

    // use  to dispose
    public void Dispose()
    {
        if (this.IsDisposed) return;
        this.IsDisposed = true;
        Renderer.Instance.DisposeWhenIdle(this.VertexBuffer);
        Renderer.Instance.DisposeWhenIdle(this.IndexBuffer);
    }
}