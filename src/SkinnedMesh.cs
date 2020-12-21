using System.Collections.Generic;
using System.Numerics;

namespace LifeSim
{
    public class SkinnedMeshData : MeshData
    {
        public readonly IList<Vector4> joints;
        public readonly IList<Vector4> weights;
        public readonly IList<Matrix4x4> inverseBindMatrices;

        public SkinnedMeshData(
            IList<Vector3> positions, IList<ushort> indices, IList<Vector2>? uvs, IList<Vector3>? normals, 
            IList<Vector4> joints, IList<Vector4> weights, IList<Matrix4x4> inverseBindMatrices
        ) 
            : base(positions, indices, uvs, normals)
        {
            this.joints = joints;
            this.weights = weights;
            this.inverseBindMatrices = inverseBindMatrices;

            System.Console.WriteLine("Joints");
            foreach (var joint in joints) {
                System.Console.WriteLine(joint);
            }
            System.Console.WriteLine("Weights");
            foreach (var weight in weights) {
                System.Console.WriteLine(weight);
            }
            System.Console.WriteLine("InverseBindMatrices");
            foreach (var inverseBindMatrix in inverseBindMatrices) {
                System.Console.WriteLine(inverseBindMatrix);
            }
        }
    }
}