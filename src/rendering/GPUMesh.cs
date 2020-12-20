using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace LifeSim.Rendering
{
    public class GPUMesh : System.IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        struct VertData
        {
            public const uint sizeInBytes = (3 + 3 + 2) * 4;
            public Vector3 position;
            public Vector3 normal;
            public Vector2 uv;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SkinnedVertData
        {
            public const uint sizeInBytes = (3 + 3 + 2 + 4 + 4) * 4;
            public Vector3 position;
            public Vector3 normal;
            public Vector2 uv;
            public Vector4 joints;
            public Vector4 weights;
        }

        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        public uint vertexCount;
        public uint indexCount;

        public GPUMesh(ResourceFactory factory, GraphicsDevice graphicsDevice, LifeSim.Mesh mesh)
        {
            var indices = mesh.indices.ToArray();

            this.vertexCount = (uint) mesh.positions.Count;
            this.indexCount  = (uint) mesh.indices.Count;

            this._indexBuffer  = factory.CreateBuffer(new BufferDescription(this.indexCount  * sizeof(ushort)      , BufferUsage.IndexBuffer ));
            graphicsDevice.UpdateBuffer(this._indexBuffer, 0, indices);

            if (mesh is LifeSim.SkinnedMesh skinnedMesh) {
                var size = SkinnedVertData.sizeInBytes * this.vertexCount;
                this._vertexBuffer = factory.CreateBuffer(new BufferDescription(size, BufferUsage.VertexBuffer));
                graphicsDevice.UpdateBuffer(this._vertexBuffer, 0, this.GetSkinnedVerts(skinnedMesh));
            } else {
                var size = VertData.sizeInBytes * this.vertexCount;
                this._vertexBuffer = factory.CreateBuffer(new BufferDescription(size, BufferUsage.VertexBuffer));
                graphicsDevice.UpdateBuffer(this._vertexBuffer, 0, this.GetVerts(mesh));
            }

        }

        private VertData[] GetVerts(LifeSim.Mesh mesh)
        {
            VertData[] vertices = new VertData[mesh.positions.Count];
            for(var i = 0; i < mesh.positions.Count; i++) {
                vertices[i].position = mesh.positions[i];
                vertices[i].normal   = mesh.normals[i];
                vertices[i].uv       = mesh.uvs[i];
            }
            return vertices;
        }


        private SkinnedVertData[] GetSkinnedVerts(LifeSim.SkinnedMesh mesh)
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

        ~GPUMesh() {
            this.Dispose();
        }
    }
}