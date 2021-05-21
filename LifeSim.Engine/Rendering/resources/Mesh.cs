using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public class Mesh : IGeometry, System.IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct VertData
        {           
            public Vector3 position;
            public Vector3 normal;
            public Vector2 uv;
        }

        public uint vertexCount;
        public uint indexCount;
        public VertexFormat vertexFormat => VertexFormat.Regular;
        public DeviceBuffer vertexBuffer => this._vertexBuffer;
        public DeviceBuffer indexBuffer => this._indexBuffer;
        public BoundingBox aabb;

        private readonly DeviceBuffer _vertexBuffer;
        private readonly DeviceBuffer _indexBuffer;

        public Mesh(GraphicsDevice graphicsDevice, MeshData mesh)
        {
            var indices = mesh.indices.ToArray();

            this.vertexCount = (uint) mesh.positions.Count;
            this.indexCount  = (uint) mesh.indices.Count;

            var factory = graphicsDevice.ResourceFactory;
            this._indexBuffer  = factory.CreateBuffer(new BufferDescription(this.indexCount * sizeof(ushort), BufferUsage.IndexBuffer));
            graphicsDevice.UpdateBuffer(this._indexBuffer, 0, indices);

            uint size = (uint) Marshal.SizeOf<VertData>() * this.vertexCount;
            this._vertexBuffer = factory.CreateBuffer(new BufferDescription(size, BufferUsage.VertexBuffer));
            graphicsDevice.UpdateBuffer(this._vertexBuffer, 0, this.GetVerts(mesh));

            this.aabb = BoundingBox.CreateFromVertices(mesh.positions.ToArray());
        }

        private VertData[] GetVerts(MeshData mesh)
        {
            VertData[] vertices = new VertData[mesh.positions.Count];
            for(var i = 0; i < mesh.positions.Count; i++) {
                vertices[i].position = mesh.positions[i];
                vertices[i].normal   = mesh.normals[i];
                vertices[i].uv       = mesh.uvs[i];
            }
            return vertices;
        }

        public void Dispose()
        {
            this._vertexBuffer.Dispose();
            this._indexBuffer.Dispose();
        }

        ~Mesh() {
            this.Dispose();
        }
    }
}