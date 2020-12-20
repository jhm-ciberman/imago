using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;

namespace LifeSim.Rendering
{
    public class GPUMesh : System.IDisposable
    {
        public struct VertData
        {
            public Vector3 position;
            public Vector3 normal;
            public Vector2 uv;
            public VertData(Vector3 position, Vector3 normal, Vector2 uv)
            {
                this.position = position;
                this.normal = normal;
                this.uv = uv;
            }
            public const uint sizeInBytes = (3 + 3 + 2) * 4;
        }

        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        public uint indexCount;

        public GPUMesh(ResourceFactory factory, GraphicsDevice graphicsDevice, LifeSim.Mesh mesh)
        {
            VertData[] vertices = new VertData[mesh.positions.Count];
            for(var i = 0; i < mesh.positions.Count; i++) {
                vertices[i] = new GPUMesh.VertData(mesh.positions[i],  mesh.normals[i], mesh.uvs[i]);
            }
            var indices = mesh.indices.ToArray();
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

        ~GPUMesh() {
            this.Dispose();
        }
    }
}