using System;
using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph;

public class RenderNode3D : Node3D
{
    private Renderable? _renderable = null;
    public Renderable? Renderable
    {
        get => this._renderable;
        set
        {
            if (this._renderable == value) return;

            if (this._renderable != null)
            {
                this.Scene?.NotifyRenderableRemoved(this._renderable);
            }

            this._renderable = value;

            if (this._renderable != null)
            {
                if (!this.TransformIsDirty)
                {
                    this._renderable.SetTransform(ref this._worldMatrix);
                }

                this.Scene?.NotifyRenderableAdded(this._renderable);
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

    protected override void _AttachToSceneRecursive(Scene scene)
    {
        base._AttachToSceneRecursive(scene);

        if (this.Renderable != null)
        {
            scene.NotifyRenderableAdded(this.Renderable);
        }
    }

    protected override void _DetachFromSceneRecursive()
    {
        if (this.Renderable != null)
        {
            this.Scene?.NotifyRenderableRemoved(this.Renderable);
        }

        base._DetachFromSceneRecursive();
    }
}