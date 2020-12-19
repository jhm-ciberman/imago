using System.Collections.Generic;
using System.Numerics;

namespace LifeSim
{
    public class MeshData
    {
        public readonly IReadOnlyList<Vector3> positions;
        public readonly IReadOnlyList<Vector3> normals;
        public readonly IReadOnlyList<Vector2> uvs;
        public readonly IReadOnlyList<ushort> indices;
        
        public MeshData(IReadOnlyList<Vector3> positions, IReadOnlyList<Vector2> uvs, IReadOnlyList<ushort> indices) 
        {
            this.positions = positions;
            this.uvs = uvs;
            this.indices = indices;
            this.normals = this._ComputeNormals(positions, indices);
        }

        public MeshData(IReadOnlyList<Vector3> positions, IReadOnlyList<Vector2> uvs, IReadOnlyList<ushort> indices, IReadOnlyList<Vector3> normals) 
        {
            this.positions = positions;
            this.uvs = uvs;
            this.normals = normals;
            this.indices = indices;
        }

        private Vector3[] _ComputeNormals(IReadOnlyList<Vector3> positions, IReadOnlyList<ushort> indices)
        {
            Vector3[] normals = new Vector3[positions.Count];

            for (var i = 0; i < indices.Count; i += 3) {
                var index1 = indices[i + 0];
                var index2 = indices[i + 1];
                var index3 = indices[i + 2];

                var p1 = positions[index1];
                var p2 = positions[index2];
                var p3 = positions[index3];

                var normal = Vector3.Cross((p3 - p2), (p1 - p2));

                normals[index1] = normal;
                normals[index2] = normal;
                normals[index3] = normal;
            }

            return normals;
        }

    }
}