using System.Numerics;
using Veldrid;

namespace LifeSim.Rendering
{
    public class Mesh : System.IDisposable
    {
        public struct VertData
        {
            public Vector3 position;
            public Vector2 uv;
            public RgbaFloat color;
            public VertData(Vector3 position, Vector2 uv, RgbaFloat color)
            {
                this.position = position;
                this.uv = uv;
                this.color = color;
            }
            public const uint sizeInBytes = (3 + 2 + 4) * 4;
        }

        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        public uint indexCount;

        public Mesh(ResourceFactory factory, GraphicsDevice graphicsDevice, VertData[] vertices, ushort[] indices)
        {
            uint vertCount = (uint) vertices.Length;
            uint indexCount = (uint) indices.Length;
            this._vertexBuffer = factory.CreateBuffer(new BufferDescription(vertCount * VertData.sizeInBytes, BufferUsage.VertexBuffer));
            this._indexBuffer = factory.CreateBuffer(new BufferDescription(indexCount * sizeof(ushort), BufferUsage.IndexBuffer));

            graphicsDevice.UpdateBuffer(this._vertexBuffer, 0, vertices);
            graphicsDevice.UpdateBuffer(this._indexBuffer, 0, indices);

            this.indexCount = indexCount;
        }

        public DeviceBuffer vertexBuffer => this._vertexBuffer;
        public DeviceBuffer indexBuffer => this._indexBuffer;

        public void Dispose()
        {
            this._vertexBuffer.Dispose();
            this._indexBuffer.Dispose();
        }
    }
}