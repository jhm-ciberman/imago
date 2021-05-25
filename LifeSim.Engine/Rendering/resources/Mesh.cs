using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public partial class Mesh : IGeometry, System.IDisposable
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

        public virtual void Dispose()
        {
            this.vertexBuffer.Dispose();
            this.indexBuffer.Dispose();
        }
    }
}