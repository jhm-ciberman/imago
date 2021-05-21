using System;
using System.Numerics;
using LifeSim.Engine.Rendering;
using Veldrid.Utilities;

namespace LifeSim.Engine.SceneGraph
{
    public class RenderNode3D : Node3D
    {
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

        protected void _SetInstanceData<T>(ref T data) where T : struct
        {
            this._renderable?.SetInstanceData<T>(ref data);
        }

        internal virtual void UpdateInstanceData()
        {
            // Nothing should be implemented in each child
        }
    }
}