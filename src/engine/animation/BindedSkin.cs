using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Anim
{
    public class BindedSkin
    {
        public readonly IList<Node3D> joints = new List<Node3D>();
        public readonly IList<Matrix4x4> inverseBindMatrices;
        
        public BindedSkin(IList<Node3D> joints, IList<Matrix4x4> inverseBindMatrices)
        {
            this.joints = joints;
            this.inverseBindMatrices = inverseBindMatrices;
        }
    }
}