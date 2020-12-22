using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Rendering
{
    public class Skin
    {
        public readonly Node3D root;
        public readonly IList<Matrix4x4> inverseBindMatrices;

        public Skin(Node3D root, IList<Matrix4x4> inverseBindMatrices)
        {
            this.root = root;
            this.inverseBindMatrices = inverseBindMatrices;
        }
    }
}