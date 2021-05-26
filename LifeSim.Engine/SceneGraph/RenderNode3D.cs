using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Anim;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class RenderNode3D : Node3D
    {
        private uint _pickingID = 0;
        public uint pickingID
        {
            get => this._pickingID;
            set { this._pickingID = value; if (this._renderable != null) { this._renderable.pickingID = value; }}
        }

        private Mesh? _mesh = null;
        public Mesh? mesh
        {
            get => this._mesh;
            set
            {
                this._mesh = value;
                this._OnTransformDirty();
            }
        }
        
        private BindedSkin? _skin = null;
        public BindedSkin? skin
        {
            get => this._skin;
            set
            {
                if (value != this._skin) {
                    this._skin = value;
                    if (value != null) {
                        
                    }
                }
            }
        }

        private SurfaceMaterial? _material = null;
        public SurfaceMaterial? material
        {
            get => this._material;
            set => this._material = value;
        }

        internal Renderable? _renderable = null;

        public RenderNode3D()
        {
            //
        }

        public RenderNode3D(Mesh? mesh = null, SurfaceMaterial? material = null)
        {
            this.mesh = mesh;
            this.material = material;
        }

        protected void _SetInstanceDataDirty()
        {
            this.scene?._OnInstanceDataDirty(this);
        }

        protected override void _AfterMatrixUpdate()
        {
            this._renderable?.UpdateTransform(ref this.worldMatrix);
        }

        public void SetInstanceData<T>(ref T data) where T : struct
        {
            this._renderable?.SetInstanceData<T>(ref data);
        }

        protected internal virtual void UpdateInstanceData()
        {
            // Nothing should be implemented in each child
        }

        public void Update()
        {
            if (this.skin == null || this._renderable == null) return;

            Matrix4x4.Invert(this.worldMatrix, out Matrix4x4 inverseMeshWorldMatrix);
            var joints = this.skin.joints;
            var invBindMatrices = this.skin.inverseBindMatrices;
            var skeleton = this._renderable.skeleton;
            for (int i = 0; i < joints.Count; i++) {
                skeleton.bonesMatrices[i] = invBindMatrices[i] * joints[i].worldMatrix * inverseMeshWorldMatrix;
            }
        }
    }
}