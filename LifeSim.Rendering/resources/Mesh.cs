using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Rendering
{
    public class Mesh : System.IDisposable
    {
        private static int _count = 0;

        public int Id { get; private set; }
        public uint IndexCount { get; private set; }
        public VertexFormat VertexFormat { get; private set; }
        public DeviceBuffer VertexBuffer { get; private set; }
        public DeviceBuffer IndexBuffer { get; private set; }
        public BoundingBox AABB { get; private set; }

        protected Mesh(VertexFormat vertexFormat, uint indexCount, Veldrid.DeviceBuffer vertexBuffer, Veldrid.DeviceBuffer indexBuffer, ref BoundingBox boundingBox)
        {
            this.Id = ++Mesh._count;
            this.VertexFormat = vertexFormat;
            this.IndexCount  = indexCount;
            this.AABB = boundingBox;
            this.IndexBuffer = indexBuffer;
            this.VertexBuffer = vertexBuffer;
        }

        public static Mesh CreateFromData(IMeshData meshData)
        {
            var gd = Renderer.GraphicsDevice;
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
            this.VertexBuffer.Dispose();
            this.IndexBuffer.Dispose();
        }
    }
}