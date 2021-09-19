using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class Skeleton : ISkeleton
    {
        public readonly IList<Node3D> Joints = new List<Node3D>();
        public readonly IList<Matrix4x4> InverseBindMatrices;

        private readonly Matrix4x4[] _bonesMatrices;

        public Skeleton(IList<Node3D> joints, IList<Matrix4x4> inverseBindMatrices)
        {
            this.Joints = joints;
            this.InverseBindMatrices = inverseBindMatrices;
            this._bonesMatrices = new Matrix4x4[this.Joints.Count];
        }

        public Matrix4x4[] BonesMatrices => this._bonesMatrices;

        public void UpdateMatrices(ref Matrix4x4 inverseMeshWorldMatrix)
        {
            for (int i = 0; i < this.Joints.Count; i++) {
                this._bonesMatrices[i] = this.InverseBindMatrices[i] * this.Joints[i].WorldMatrix * inverseMeshWorldMatrix;
            }
        }
    }
}