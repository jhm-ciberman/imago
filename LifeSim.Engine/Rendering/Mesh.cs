using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering;

public class Mesh : System.IDisposable
{
    private static int _count = 0;

    public int Id { get; private set; }
    public uint IndexCount { get; private set; }
    public VertexFormat VertexFormat { get; private set; }
    public DeviceBuffer VertexBuffer { get; private set; }
    public DeviceBuffer IndexBuffer { get; private set; }
    public BoundingBox AABB { get; private set; }

    public IMeshData MeshData { get; private set; }

    protected Mesh(VertexFormat vertexFormat, uint indexCount, Veldrid.DeviceBuffer vertexBuffer, Veldrid.DeviceBuffer indexBuffer, ref BoundingBox boundingBox, IMeshData meshData)
    {
        this.Id = ++Mesh._count;
        this.VertexFormat = vertexFormat;
        this.IndexCount = indexCount;
        this.AABB = boundingBox;
        this.IndexBuffer = indexBuffer;
        this.VertexBuffer = vertexBuffer;
        this.MeshData = meshData;
    }

    public static Mesh CreateFromData(IMeshData meshData)
    {
        var gd = Renderer.Instance.GraphicsDevice;
        BoundingBox boundingBox = BoundingBox.CreateFromVertices(meshData.Positions);

        var indexBuffer = meshData.CreateIndexBuffer(gd);
        var vertexBuffer = meshData.CreateVertexBuffer(gd);

        return new Mesh(meshData.VertexFormat, (uint)meshData.Indices.Length, vertexBuffer, indexBuffer, ref boundingBox, meshData);
    }

    public virtual void Dispose()
    {
        this.VertexBuffer.Dispose();
        this.IndexBuffer.Dispose();
    }
}