using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class Skeleton : ISkeleton
    {
        public readonly IList<Node3D> joints = new List<Node3D>();
        public readonly IList<Matrix4x4> inverseBindMatrices;

        private readonly Matrix4x4[] _bonesMatrices;

        public Skeleton(IList<Node3D> joints, IList<Matrix4x4> inverseBindMatrices)
        {
            this.joints = joints;
            this.inverseBindMatrices = inverseBindMatrices;
            this._bonesMatrices = new Matrix4x4[this.joints.Count];
        }

        public Matrix4x4[] bonesMatrices => this._bonesMatrices;

        public void UpdateMatrices(ref Matrix4x4 inverseMeshWorldMatrix)
        {
            for (int i = 0; i < this.joints.Count; i++) {
                this._bonesMatrices[i] = this.inverseBindMatrices[i] * joints[i].worldMatrix * inverseMeshWorldMatrix;
            }
        }
    }
}