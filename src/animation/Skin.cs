using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Anim
{
    public class Skin
    {
        public readonly string? root;
        public readonly IList<string> jointNames;
        public readonly IList<Matrix4x4> inverseBindMatrices;

        public Skin(IList<Matrix4x4> inverseBindMatrices, IList<string> jointNames, string? root)
        {
            this.root = root;
            this.jointNames = jointNames;
            this.inverseBindMatrices = inverseBindMatrices;
        }
    }
}