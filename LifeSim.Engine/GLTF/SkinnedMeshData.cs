using System.Buffers;
using System.Numerics;
using LifeSim.Rendering;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Engine.GLTF
{
    public class SkinnedMeshData : BaseMeshData<SkinnedVertex>
    {
        public SkinnedMeshData(ushort[] indices, SkinnedVertex[] vertices) : base(indices, vertices)
        {
        }

        public static SkinnedMeshData CreateMesh(ushort[] indices, Vector3[] positions, Vector3[]? normals, Vector2[]? uvs, Vector4UShort[] joints, Vector4[] weights)
        {
            SkinnedVertex[] vertices = ArrayPool<SkinnedVertex>.Shared.Rent(positions.Length);
            for(var i = 0; i < positions.Length; i++) {
                vertices[i].position = positions[i];
                vertices[i].joints = joints[i];
                vertices[i].weights = weights[i];
            }
            if (normals != null) {
                for(var i = 0; i < normals.Length; i++) {
                    vertices[i].normal = normals[i];
                }
            }
            if (uvs != null) {
                for(var i = 0; i < uvs.Length; i++) {
                    vertices[i].uv = uvs[i];
                }
            }
            var mesh = new SkinnedMeshData(indices, vertices);
            ArrayPool<SkinnedVertex>.Shared.Return(vertices);
            return mesh;
        }

        protected override VertexFormat MakeVertexFormat()
        {
            return SkinnedVertex.vertexFormat;
        }

        protected override Vector3 GetPosition(int index)
        {
            return this.vertices[index].position;
        }
    }
}