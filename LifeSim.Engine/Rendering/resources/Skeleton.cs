using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Rendering
{
    public class Skeleton
    {
        public const int MAX_NUMBER_OF_BONES = 64;

        public readonly IList<Node3D> joints = new List<Node3D>();
        public readonly IList<Matrix4x4> inverseBindMatrices;

        public Skeleton(IList<Node3D> joints, IList<Matrix4x4> inverseBindMatrices)
        {
            this.joints = joints;
            this.inverseBindMatrices = inverseBindMatrices;
        }
    }
}