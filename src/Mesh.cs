using System.Collections.Generic;
using System.Numerics;

namespace LifeSim
{
    public class Mesh
    {
        public readonly IList<Vector3> positions;
        public readonly IList<Vector3> normals;
        public readonly IList<Vector2> uvs;
        public readonly IList<ushort> indices;
        
        public Mesh(IList<Vector3> positions, IList<ushort> indices, IList<Vector2> uvs, IList<Vector3> normals) 
        {
            this.positions = positions;
            this.uvs = uvs ?? new Vector2[positions.Count];
            this.normals = normals ?? this._ComputeNormals(positions, indices);
            this.indices = indices;
        }

        private Vector3[] _ComputeNormals(IList<Vector3> positions, IList<ushort> indices)
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