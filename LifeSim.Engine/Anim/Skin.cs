using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Engine.Anim
{
    public class Skin
    {
        public readonly string? Root;
        public readonly IList<string> JointNames;
        public readonly IList<Matrix4x4> InverseBindMatrices;

        public Skin(IList<Matrix4x4> inverseBindMatrices, IList<string> jointNames, string? root)
        {
            this.Root = root;
            this.JointNames = jointNames;
            this.InverseBindMatrices = inverseBindMatrices;
        }
    }
}