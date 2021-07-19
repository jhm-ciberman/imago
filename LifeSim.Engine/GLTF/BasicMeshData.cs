using System.Buffers;
using System.Numerics;
using LifeSim.Rendering;
using Veldrid;

namespace LifeSim.Engine.GLTF
{
    public class BasicMeshData : BaseMeshData<BasicVertex>
    {
        public BasicMeshData(ushort[] indices, BasicVertex[] vertices) : base(indices, vertices)
        {
        }

        public static BasicMeshData CreateMesh(ushort[] indices, Vector3[] positions, Vector3[]? normals, Vector2[]? uvs)
        {
            BasicVertex[] vertices = ArrayPool<BasicVertex>.Shared.Rent(positions.Length);
            for(var i = 0; i < positions.Length; i++) {
                vertices[i].position = positions[i];
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
            var mesh = new BasicMeshData(indices, vertices);
            ArrayPool<BasicVertex>.Shared.Return(vertices);
            return mesh;
        }


        public void Translate(Vector3 translation)
        {
            for (var i = 0; i < this.vertices.Length; i++) {
                this.vertices[i].position += translation;
            }
        }

        public BasicMeshData Clone()
        {
            ushort[] indices = (ushort[]) this.indices.Clone();
            BasicVertex[] vertices = (BasicVertex[]) this.vertices.Clone();

            return new BasicMeshData(indices, vertices);
        }

        public void RecomputeNormals()
        {
            for (var i = 0; i < this.indices.Length; i += 3) {
                ushort index1 = this.indices[i + 0];
                ushort index2 = this.indices[i + 1];
                ushort index3 = this.indices[i + 2];

                Vector3 p1 = this.vertices[index1].position;
                Vector3 p2 = this.vertices[index2].position;
                Vector3 p3 = this.vertices[index3].position;

                Vector3 normal = Vector3.Cross((p3 - p2), (p1 - p2));

                this.vertices[index1].normal = normal;
                this.vertices[index2].normal = normal;
                this.vertices[index3].normal = normal;
            }
        }

        
        public void FlipNormals()
        {
            for (int i = 0; i < this.vertices.Length; i++) {
                this.vertices[i].normal = -this.vertices[i].normal;
            }
        }

        public void FlipFaces()
        {
            this.FlipIndices();
            this.FlipNormals();
        }

        public BasicMeshData Merge(BasicMeshData mesh)
        {
            BasicVertex[] vertices = new BasicVertex[mesh.vertices.Length + this.vertices.Length];
            ushort[] indices = new ushort[mesh.indices.Length + this.indices.Length];
            
            for (int i = 0; i < this.vertices.Length; i++) {
                vertices[i] = this.vertices[i];
            }

            for (int i = 0; i < mesh.vertices.Length; i++) {
                int j = i + this.vertices.Length;
                vertices[j] = mesh.vertices[i];
            }

            for (int i = 0; i < this.indices.Length; i++) {
                indices[i] = this.indices[i];
            }

            for (int i = 0; i < mesh.indices.Length; i++) {
                int j = i + this.indices.Length;
                indices[j] = (ushort) ((int)mesh.indices[i] + this.vertices.Length);
            }
            
            return new BasicMeshData(indices, vertices);
        }

        protected override Vector3 GetPosition(int index)
        {
            return this.vertices[index].position;
        }

        protected override VertexFormat MakeVertexFormat()
        {
            return BasicVertex.vertexFormat;
        }
    }
}