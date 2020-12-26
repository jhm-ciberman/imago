using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Rendering
{
    public class Skin
    {
        public readonly Node3D? root;
        public readonly IList<Node3D> joints;
        public readonly IList<Matrix4x4> inverseBindMatrices;

        public Skin(IList<Matrix4x4> inverseBindMatrices, IList<Node3D> joints, Node3D? root)
        {
            this.root = root;
            this.joints = joints;
            this.inverseBindMatrices = inverseBindMatrices;

            for (int i = 0; i < this.joints.Count; i++) {
                //Matrix4x4.Invert(this.joints[i].worldMatrix, out Matrix4x4 m);
                System.Console.WriteLine(this.inverseBindMatrices[i]);
            }
        }
    }
}