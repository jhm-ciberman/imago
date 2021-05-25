using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public class SkinnedMeshData : MeshData, IMeshFactory
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct SkinnedVertData
        {
            public Vector3 position;
            public Vector3 normal;
            public Vector2 uv;
            public UShort4 joints;
            public Vector4 weights;

            public SkinnedVertData(Vector3 position, Vector3 normal, Vector2 uv, UShort4 joints, Vector4 weights)
            {
                this.position = position;
                this.normal = normal;
                this.uv = uv;
                this.joints = joints;
                this.weights = weights;
            }
        }

        public readonly IList<UShort4> joints;
        public readonly IList<Vector4> weights;

        public SkinnedMeshData(
            IList<Vector3> positions, IList<ushort> indices, IList<Vector2>? uvs, IList<Vector3>? normals, 
            IList<UShort4> joints, IList<Vector4> weights
        ) 
            : base(positions, indices, uvs, normals)
        {
            this.joints = joints;
            this.weights = weights;
        }

        public override Mesh CreateMesh(IRenderingResourcesFactory meshFactory)
        {
            var boundingBox = BoundingBox.CreateFromVertices(this.positions.ToArray());

            SkinnedVertData[] vertices = ArrayPool<SkinnedVertData>.Shared.Rent(this.positions.Count);
            for(var i = 0; i < this.positions.Count; i++) {
                vertices[i] = new SkinnedVertData(this.positions[i], this.normals[i], this.uvs[i], this.joints[i], this.weights[i]);
            }
            var mesh = meshFactory.CreateMesh(ShaderRegistry.skinnedVertexFormat, vertices, this.indices.ToArray(), ref boundingBox);

            ArrayPool<SkinnedVertData>.Shared.Return(vertices);

            return mesh;
        }
    }
}