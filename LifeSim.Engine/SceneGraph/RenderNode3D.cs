using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.Resources;

namespace LifeSim.Engine.SceneGraph;

public partial class RenderNode3D : Node3D
{
    // Contiguos layout
    [StructLayout(LayoutKind.Sequential)]
    private struct InstanceData
    {
        public Vector4 AlbedoColor { get; set; } = new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
        public Vector4 TextureST { get; set; } = new Vector4(1.0f, 1.0f, 0.0f, 0.0f);
    }

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

    private bool _visible = true;

    public bool Visible
    {
        get => this._visible;
        set
        {
            if (this._visible == value) return;
            this._visible = value;
            this.RenderQueueFlagsChanged();
        }
    }

    private ShadowCasting _shadowCastingMode = ShadowCasting.CastShadows;

    public ShadowCasting ShadowCastingMode
    {
        get => this._shadowCastingMode;
        set
        {
            if (this._shadowCastingMode == value) return;
            this._shadowCastingMode = value;
            this.RenderQueueFlagsChanged();
        }
    }

    private InstanceData _instanceData;

    public RenderNode3D()
    {
        this._instanceData = new InstanceData();

        this._renderable = Renderer.Instance.MakeRenderable(Marshal.SizeOf<InstanceData>());
        this._renderable.SetInstanceData(this._instanceData);
    }

    public RenderNode3D(Mesh mesh) : this()
    {
        this.Mesh = mesh;
    }

    public ColorF AlbedoColor
    {
        get => this._instanceData.AlbedoColor;
        set { this._instanceData.AlbedoColor = value; this.OnInstanceDataDirty(); }
    }

    public Vector4 TextureST
    {
        get => this._instanceData.TextureST;
        set { this._instanceData.TextureST = value; this.OnInstanceDataDirty(); }
    }

    private void OnInstanceDataDirty()
    {
        this._renderable.SetInstanceData(this._instanceData);
    }

    public override void UpdateWorldMatrix(ref Matrix4x4 parentMatrix)
    {
        base.UpdateWorldMatrix(ref parentMatrix);

        this._renderable.SetTransform(ref this.WorldMatrix);
    }

    protected override void AttachToSceneRecursive(Scene scene)
    {
        base.AttachToSceneRecursive(scene);

        scene.NotifyRenderableAdded(this._renderable);
        scene.RegisterPickingId(this._renderable.PickingId, this);
    }

    protected override void DetachFromSceneRecursive()
    {
        if (this.Scene is null) return;
        this.Scene.NotifyRenderableRemoved(this._renderable);
        this.Scene.UnregisterPickingId(this._renderable.PickingId);

        base.DetachFromSceneRecursive();
    }

    private void RenderQueueFlagsChanged()
    {
        RenderQueueFlags flags = RenderQueueFlags.None;

        if (this.Visible)
        {
            flags |= RenderQueueFlags.Opaque;
        }

        if (this.ShadowCastingMode == ShadowCasting.CastShadows || this.ShadowCastingMode == ShadowCasting.OnlyShadows)
        {
            flags |= RenderQueueFlags.ShadowCaster;
        }

        this._renderable.RenderQueueFlags = flags;
    }
}