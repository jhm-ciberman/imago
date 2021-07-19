using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Rendering
{
    public class Mesh : System.IDisposable
    {
        static int _count = 0;

        public int id { get ; private set; }
        public uint indexCount { get ; private set; }
        public VertexFormat vertexFormat { get; private set; }
        public DeviceBuffer vertexBuffer { get; private set; }
        public DeviceBuffer indexBuffer { get; private set; }
        public BoundingBox aabb { get ; private set; }

        protected Mesh(VertexFormat vertexFormat, uint indexCount, Veldrid.DeviceBuffer vertexBuffer, Veldrid.DeviceBuffer indexBuffer, ref BoundingBox boundingBox)
        {
            this.id = ++Mesh._count;
            this.vertexFormat = vertexFormat;
            this.indexCount  = indexCount;
            this.aabb = boundingBox;
            this.indexBuffer = indexBuffer;
            this.vertexBuffer = vertexBuffer;
        }

        public static Mesh CreateFromData(IMeshData meshData)
        {
            var gd = Renderer.graphicsDevice;
            var factory = gd.ResourceFactory;

            var boundingBox = meshData.GetBoundingBox();
            var vertexFormat = meshData.GetVertexFormat();

            var cl = factory.CreateCommandList();
            cl.Begin();
            var indexBuffer = meshData.CreateIndexBuffer(factory, cl, out int indexCount);
            var vertexBuffer = meshData.CreateVertexBuffer(factory, cl);
            cl.End();
            gd.SubmitCommands(cl);
            cl.Dispose();
            
            return new Mesh(vertexFormat, (uint) indexCount, vertexBuffer, indexBuffer, ref boundingBox);
        }

        public virtual void Dispose()
        {
            this.vertexBuffer.Dispose();
            this.indexBuffer.Dispose();
        }
    }
}