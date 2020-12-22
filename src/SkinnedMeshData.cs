using System.Collections.Generic;
using System.Numerics;

namespace LifeSim
{
    public class SkinnedMeshData : MeshData
    {
        public readonly IList<Vector4> joints;
        public readonly IList<Vector4> weights;


        public SkinnedMeshData(
            IList<Vector3> positions, IList<ushort> indices, IList<Vector2>? uvs, IList<Vector3>? normals, 
            IList<Vector4> joints, IList<Vector4> weights
        ) 
            : base(positions, indices, uvs, normals)
        {
            this.joints = joints;
            this.weights = weights;
        }
    }
}