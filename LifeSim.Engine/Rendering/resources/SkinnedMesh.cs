using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public class SkinnedMesh : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct SkinnedVertData
        {
            public Vector3 position;
            public Vector3 normal;
            public Vector2 uv;
            public UShort4 joints;
            public Vector4 weights;
        }

        public uint vertexCount;
        public uint indexCount;
        public VertexFormat vertexFormat => VertexFormat.Skinned;

        private readonly DeviceBuffer _vertexBuffer;
        private readonly DeviceBuffer _indexBuffer;
        public BoundingBox boundingBox;

        public SkinnedMesh(GraphicsDevice graphicsDevice, SkinnedMeshData mesh)
        {
            var indices = mesh.indices.ToArray();

            this.vertexCount = (uint) mesh.positions.Count;
            this.indexCount  = (uint) mesh.indices.Count;

            var factory = graphicsDevice.ResourceFactory;
            this._indexBuffer  = factory.CreateBuffer(new BufferDescription(this.indexCount * sizeof(ushort), BufferUsage.IndexBuffer));
            graphicsDevice.UpdateBuffer(this._indexBuffer, 0, indices);

            uint size = (uint) Marshal.SizeOf<SkinnedVertData>() * this.vertexCount;
            this._vertexBuffer = factory.CreateBuffer(new BufferDescription(size, BufferUsage.VertexBuffer));
            graphicsDevice.UpdateBuffer(this._vertexBuffer, 0, this.GetSkinnedVerts(mesh));

            this.boundingBox = BoundingBox.CreateFromVertices(mesh.positions.ToArray());
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

        public void Dispose()
        {
            this._vertexBuffer.Dispose();
            this._indexBuffer.Dispose();
        }
    }
}