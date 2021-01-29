using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Engine.Rendering
{
    public class MeshData
    {
        public readonly IList<Vector3> positions;
        public readonly IList<Vector3> normals;
        public readonly IList<Vector2> uvs;
        public readonly IList<ushort> indices;
        
        public MeshData(IList<Vector3> positions, IList<ushort> indices, IList<Vector2>? uvs = null, IList<Vector3>? normals = null) 
        {
            this.positions = positions;
            this.uvs = uvs ?? new Vector2[positions.Count];
            this.normals = normals ?? new Vector3[positions.Count]; 
            this.indices = indices;

            if (this.positions.Count != this.uvs.Count) { // TODO: remove this assertion
                throw new System.Exception("UVS " + this.uvs.Count + " " + this.positions.Count);
            }
            if (this.positions.Count != this.normals.Count) {
                throw new System.Exception("NORMALS");
            }

            if (normals == null) {
                this.RecomputeNormals();
            }
        }

        public void RecomputeNormals()
        {
            for (var i = 0; i < this.indices.Count; i += 3) {
                var index1 = this.indices[i + 0];
                var index2 = this.indices[i + 1];
                var index3 = this.indices[i + 2];

                var p1 = this.positions[index1];
                var p2 = this.positions[index2];
                var p3 = this.positions[index3];

                var normal = Vector3.Cross((p3 - p2), (p1 - p2));

                this.normals[index1] = normal;
                this.normals[index2] = normal;
                this.normals[index3] = normal;
            }
        }

        public void FlipIndices()
        {
            for (var i = 0; i < this.indices.Count; i += 3) {
                var a = this.indices[i + 0];
                var b = this.indices[i + 1];
                var c = this.indices[i + 2];

                this.indices[i + 0] = c;
                this.indices[i + 1] = b;
                this.indices[i + 2] = a;
            }

        }

        public void FlipNormals()
        {
            for (int i = 0; i < this.normals.Count; i++) {
                this.normals[i] = -this.normals[i];
            }
        }

        public void FlipFaces()
        {
            this.FlipIndices();
            this.FlipNormals();
        }

        public void Translate(Vector3 translation)
        {
            for (var i = 0; i < this.positions.Count; i++) {
                this.positions[i] += translation;
            }
        }

        public MeshData Clone()
        {
            Vector3[] positions    = new Vector3[this.positions.Count];
            Vector3[] normals      = new Vector3[this.normals.Count];
            Vector2[] uvs          = new Vector2[this.uvs.Count];
            ushort[] indices = new ushort[this.indices.Count];

            for (var i = 0; i < this.indices.Count; i++) {
                indices[i] = this.indices[i];
            }

            for (var i = 0; i < this.positions.Count; i++) {
                positions[i] = this.positions[i];
            }

            for (var i = 0; i < this.normals.Count; i++) {
                normals[i] = this.normals[i];
            }

            for (int i = 0; i < this.uvs.Count; i++) {
                uvs[i] = this.uvs[i];
            }

            return new MeshData(positions, indices, uvs, normals);
        }

        public MeshData Merge(MeshData mesh)
        {
            Vector3[] positions = new Vector3[mesh.positions.Count + this.positions.Count];
            Vector3[] normals   = new Vector3[mesh.normals.Count   + this.normals.Count];
            Vector2[] uvs       = new Vector2[mesh.uvs.Count       + this.uvs.Count];
            ushort[] indices    = new ushort[mesh.indices.Count    + this.indices.Count];
            
            for (int i = 0; i < this.positions.Count; i++) {
                positions[i] = this.positions[i];
                normals[i]   = this.normals[i];
                uvs[i]       = this.uvs[i];
            }

            for (int i = 0; i < mesh.positions.Count; i++) {
                int j = i + this.positions.Count;
                positions[j] = mesh.positions[i];
                normals[j]   = mesh.normals[i];
                uvs[j]       = mesh.uvs[i];
            }

            for (int i = 0; i < this.indices.Count; i++) {
                indices[i] = this.indices[i];
            }

            for (int i = 0; i < mesh.indices.Count; i++) {
                int j = i + this.indices.Count;
                indices[j] = (ushort) ((int)mesh.indices[i] + this.positions.Count);
            }
            
            return new MeshData(positions, indices, uvs, normals);
        }

    }
}