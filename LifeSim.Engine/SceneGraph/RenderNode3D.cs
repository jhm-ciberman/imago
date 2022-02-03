using System;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.Resources;

namespace LifeSim.Engine.SceneGraph;

public class RenderNode3D : Node3D
{
    private readonly Renderable _renderable;

    private Mesh? _mesh;
    public Mesh? Mesh
    {
        get => this._mesh;
        set
        {
            this._mesh = value;

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            this._renderable.SetMesh(value);
        }
    }

    private Material? _material;
    public Material? Material
    {
        get => this._material;
        set
        {
            this._material = value;

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            this._renderable.SetMaterial(value);
        }
    }



    private Skeleton? _skeleton;
    public Skeleton? Skeleton
    {
        get => this._skeleton;
        set
        {
            this._skeleton = value;

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            this._renderable.SetSkeleton(value);
        }
    }

    public RenderNode3D(Renderer renderer)
    {
        this._renderable = new Renderable(renderer.Storage, this);
    }

    public RenderNode3D(Renderer renderer, Mesh mesh) : this(renderer)
    {
        this.Mesh = mesh;
    }

    public void SetInstanceData<T>(string name, T data) where T : unmanaged
    {
        this._renderable.SetInstanceData(name, data);
    }

    public override void UpdateWorldMatrix(ref Matrix4x4 parentMatrix)
    {
        base.UpdateWorldMatrix(ref parentMatrix);

        this._renderable?.SetTransform(ref this.WorldMatrix);
    }

    public override RenderNode3D? FirstRenderable()
    {
        return this;
    }

    protected override void AttachToSceneRecursive(Scene scene)
    {
        base.AttachToSceneRecursive(scene);

        scene.NotifyRenderableAdded(this._renderable);
    }

    protected override void DetachFromSceneRecursive()
    {
        this.Scene?.NotifyRenderableRemoved(this._renderable);
        base.DetachFromSceneRecursive();
    }
}