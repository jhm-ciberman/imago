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
            for (var i = 0; i < positions.Length; i++)
            {
                vertices[i].Position = positions[i];
            }
            if (normals != null)
            {
                for (var i = 0; i < normals.Length; i++)
                {
                    vertices[i].Normal = normals[i];
                }
            }
            if (uvs != null)
            {
                for (var i = 0; i < uvs.Length; i++)
                {
                    vertices[i].Uv = uvs[i];
                }
            }
            var mesh = new BasicMeshData(indices, vertices);
            ArrayPool<BasicVertex>.Shared.Return(vertices);
            return mesh;
        }


        public void Translate(Vector3 translation)
        {
            for (var i = 0; i < this.Vertices.Length; i++)
            {
                this.Vertices[i].Position += translation;
            }
        }

        public BasicMeshData Clone()
        {
            ushort[] indices = (ushort[]) this.Indices.Clone();
            BasicVertex[] vertices = (BasicVertex[]) this.Vertices.Clone();

            return new BasicMeshData(indices, vertices);
        }

        public void RecomputeNormals()
        {
            for (var i = 0; i < this.Indices.Length; i += 3)
            {
                ushort index1 = this.Indices[i + 0];
                ushort index2 = this.Indices[i + 1];
                ushort index3 = this.Indices[i + 2];

                Vector3 p1 = this.Vertices[index1].Position;
                Vector3 p2 = this.Vertices[index2].Position;
                Vector3 p3 = this.Vertices[index3].Position;

                Vector3 normal = Vector3.Cross((p3 - p2), (p1 - p2));

                this.Vertices[index1].Normal = normal;
                this.Vertices[index2].Normal = normal;
                this.Vertices[index3].Normal = normal;
            }
        }


        public void FlipNormals()
        {
            for (int i = 0; i < this.Vertices.Length; i++)
            {
                this.Vertices[i].Normal = -this.Vertices[i].Normal;
            }
        }

        public void FlipFaces()
        {
            this.FlipIndices();
            this.FlipNormals();
        }

        public BasicMeshData Merge(BasicMeshData mesh)
        {
            BasicVertex[] vertices = new BasicVertex[mesh.Vertices.Length + this.Vertices.Length];
            ushort[] indices = new ushort[mesh.Indices.Length + this.Indices.Length];

            for (int i = 0; i < this.Vertices.Length; i++)
            {
                vertices[i] = this.Vertices[i];
            }

            for (int i = 0; i < mesh.Vertices.Length; i++)
            {
                int j = i + this.Vertices.Length;
                vertices[j] = mesh.Vertices[i];
            }

            for (int i = 0; i < this.Indices.Length; i++)
            {
                indices[i] = this.Indices[i];
            }

            for (int i = 0; i < mesh.Indices.Length; i++)
            {
                int j = i + this.Indices.Length;
                indices[j] = (ushort)((int)mesh.Indices[i] + this.Vertices.Length);
            }

            return new BasicMeshData(indices, vertices);
        }

        protected override Vector3 GetPosition(int index)
        {
            return this.Vertices[index].Position;
        }

        protected override VertexFormat MakeVertexFormat()
        {
            return BasicVertex.VertexFormat;
        }
    }
}