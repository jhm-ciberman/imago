using System;
using System.Numerics;
using LifeSim.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class RenderNode3D : Node3D
    {
        public event Action<Node3D, Renderable>? OnRenderableAdded;
        public event Action<Node3D, Renderable>? OnRenderableRemoved;

        private Renderable? _renderable = null;
        public Renderable? Renderable
        {
            get => this._renderable;
            set
            {
                if (this._renderable == value) return;
                
                if (this._renderable != null) {
                    this.OnRenderableRemoved?.Invoke(this, this._renderable);
                }

                this._renderable = value;

                if (this._renderable != null) {
                    if (! this.TransformIsDirty) {
                        this._renderable.SetTransform(ref this._worldMatrix);
                    }
                    this.OnRenderableAdded?.Invoke(this, this._renderable);
                }
            }
        }

        public RenderNode3D(Renderable? renderable = null)
        {
            this.Renderable = renderable;
        }

        public override void UpdateWorldMatrix(ref Matrix4x4 parentMatrix)
        {
            base.UpdateWorldMatrix(ref parentMatrix);

            this._renderable?.SetTransform(ref this.WorldMatrix);
        }

        public override Renderable? FirstRenderable()
        {
            return this.Renderable ?? base.FirstRenderable();
        }
    }
}