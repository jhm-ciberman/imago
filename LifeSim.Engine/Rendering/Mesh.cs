using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public class Mesh :  System.IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct VertData
        {           
            public Vector3 position;
            public Vector3 normal;
            public Vector2 uv;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SkinnedVertData
        {
            public Vector3 position;
            public Vector3 normal;
            public Vector2 uv;
            public UShort4 joints;
            public Vector4 weights;
        }

        private readonly DeviceBuffer _vertexBuffer;
        private readonly DeviceBuffer _indexBuffer;
        private readonly VertexLayoutKind _vertexLayoutKind;
        public VertexLayoutKind vertexLayoutKind => this._vertexLayoutKind;
        public uint vertexCount;
        public uint indexCount;

        public BoundingBox boundingBox;

        public Mesh(GraphicsDevice graphicsDevice, MeshData mesh)
        {
            var indices = mesh.indices.ToArray();

            this.vertexCount = (uint) mesh.positions.Count;
            this.indexCount  = (uint) mesh.indices.Count;

            Veldrid.ResourceFactory factory = graphicsDevice.ResourceFactory;
            this._indexBuffer  = factory.CreateBuffer(new BufferDescription(this.indexCount * sizeof(ushort), BufferUsage.IndexBuffer));
            graphicsDevice.UpdateBuffer(this._indexBuffer, 0, indices);

            if (mesh is SkinnedMeshData skinnedMesh) {
                uint size = (uint) Marshal.SizeOf<SkinnedVertData>() * this.vertexCount;
                this._vertexBuffer = factory.CreateBuffer(new BufferDescription(size, BufferUsage.VertexBuffer));
                graphicsDevice.UpdateBuffer(this._vertexBuffer, 0, this.GetSkinnedVerts(skinnedMesh));
                this._vertexLayoutKind = VertexLayoutKind.Skinned;
            } else {
                uint size = (uint) Marshal.SizeOf<VertData>() * this.vertexCount;
                this._vertexBuffer = factory.CreateBuffer(new BufferDescription(size, BufferUsage.VertexBuffer));
                graphicsDevice.UpdateBuffer(this._vertexBuffer, 0, this.GetVerts(mesh));
                this._vertexLayoutKind = VertexLayoutKind.Regular;
            }

            this.boundingBox = BoundingBox.CreateFromVertices(mesh.positions.ToArray());
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


        private SkinnedVertData[] GetSkinnedVerts(SkinnedMeshData mesh)
        {
            SkinnedVertData[] vertices = new SkinnedVertData[mesh.joints.Count];
            for(var i = 0; i < mesh.joints.Count; i++) {
                vertices[i].position = mesh.positions[i];
                vertices[i].normal   = mesh.normals[i];
                vertices[i].uv       = mesh.uvs[i];
                vertices[i].joints   = mesh.joints[i];
                vertices[i].weights  = mesh.weights[i];
            }
            return vertices;
        }

        public DeviceBuffer vertexBuffer => this._vertexBuffer;
        public DeviceBuffer indexBuffer => this._indexBuffer;

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