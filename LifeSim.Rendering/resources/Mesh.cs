using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Rendering
{
    public class Mesh : IGeometry, System.IDisposable
    {
        static int _count = 0;

        public int id { get ; private set; }
        public uint vertexCount;
        public uint indexCount;
        public VertexFormat vertexFormat { get; private set; }
        public DeviceBuffer vertexBuffer { get; private set; }
        public DeviceBuffer indexBuffer { get; private set; }
        public BoundingBox aabb;

        public Mesh(VertexFormat vertexFormat, uint vertexCount, uint indexCount, ref BoundingBox boundingBox, Veldrid.DeviceBuffer vertexBuffer, Veldrid.DeviceBuffer indexBuffer)
        {
            this.id = ++Mesh._count;
            this.vertexFormat = vertexFormat;
            this.vertexCount = vertexCount;
            this.indexCount  = indexCount;
            this.aabb = boundingBox;
            this.indexBuffer = indexBuffer;
            this.vertexBuffer = vertexBuffer;
        }

        public static Mesh Create<T>(VertexFormat vertexFormat, T[] vertices, ushort[] indices, ref BoundingBox boundingBox) where T : unmanaged
        {
            var gd = Renderer.graphicsDevice;
            uint vertexBufferSize = (uint) (Marshal.SizeOf<T>() * vertices.Length);
            DeviceBuffer vertexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(vertexBufferSize, BufferUsage.VertexBuffer));
            gd.UpdateBuffer<T>(vertexBuffer, 0, vertices);

            uint indexBufferSize = (uint) (sizeof(ushort) * indices.Length);
            DeviceBuffer indexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(indexBufferSize, BufferUsage.IndexBuffer));
            gd.UpdateBuffer<ushort>(indexBuffer, 0, indices);

            return new Mesh(vertexFormat, (uint) vertices.Length, (uint) indices.Length, ref boundingBox, vertexBuffer, indexBuffer);
        }

        public virtual void Dispose()
        {
            this.vertexBuffer.Dispose();
            this.indexBuffer.Dispose();
        }
    }
}