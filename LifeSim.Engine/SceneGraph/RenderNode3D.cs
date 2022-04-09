using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.Rendering;
using Veldrid.Utilities;

namespace LifeSim.Engine.SceneGraph;

public partial class RenderNode3D : Node3D
{
    // Contiguos layout
    [StructLayout(LayoutKind.Sequential)]
    private struct InstanceData
    {
        public InstanceData() { }
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
            if (this._mesh == value) return;
            this._mesh = value;

            if (value is not null)
            {
                this._renderable.SetMesh(value);
            }

            this.RenderQueueFlagsChanged();
        }
    }

    private Material? _material;
    public Material? Material
    {
        get => this._material;
        set
        {
            if (this._material == value) return;
            this._material = value;

            if (value is not null)
            {
                this._renderable.SetMaterial(value);
            }

            this.RenderQueueFlagsChanged();
        }
    }



    private Skeleton? _skeleton;
    public Skeleton? Skeleton
    {
        get => this._skeleton;
        set
        {
            if (this._skeleton == value) return;
            this._skeleton = value;

            if (value is not null)
            {
                this._renderable.SetSkeleton(value);
            }
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

    private bool _isPickable = false;

    public bool IsPickable
    {
        get => this._isPickable;
        set
        {
            if (this._isPickable == value) return;
            this._isPickable = value;
            if (value)
            {
                this._renderable.PickingId = Renderer.Instance.RegisterPickable(this);
            }
            else
            {
                Renderer.Instance.UnregisterPickable(this._renderable.PickingId);
                this._renderable.PickingId = 0;
            }
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
        set
        {
            if (this._instanceData.AlbedoColor == value) return;
            this._instanceData.AlbedoColor = value;
            this.OnInstanceDataDirty();
        }
    }

    public Vector4 TextureST
    {
        get => this._instanceData.TextureST;
        set
        {
            if (this._instanceData.TextureST == value) return;
            this._instanceData.TextureST = value;
            this.OnInstanceDataDirty();
        }
    }

    private void OnInstanceDataDirty()
    {
        this._renderable.SetInstanceData(this._instanceData);
    }

    public override void UpdateTransform(ref Matrix4x4 parentMatrix)
    {
        base.UpdateTransform(ref parentMatrix);

        this._renderable.SetTransform(ref this.WorldMatrix);
    }

    internal override void AttachToSceneRecursive(Scene scene)
    {
        base.AttachToSceneRecursive(scene);
        this.RenderQueueFlagsChanged();
    }

    internal override void DetachFromSceneRecursive()
    {
        if (this.Scene is null) return;
        base.DetachFromSceneRecursive();
    }

    private void RenderQueueFlagsChanged()
    {
        RenderQueueFlags flags = RenderQueueFlags.None;

        if (this.Scene is null || this.Material is null || this.Mesh is null)
        {
            this._renderable.RenderQueueFlags = flags;
            return;
        }

        if (this.Visible)
        {
            flags |= RenderQueueFlags.Opaque;

            if (this.ShadowCastingMode == ShadowCasting.CastShadows)
            {
                flags |= RenderQueueFlags.ShadowCaster;
            }
        }
        else if (this.ShadowCastingMode == ShadowCasting.OnlyShadows)
        {
            flags |= RenderQueueFlags.ShadowCaster;
        }

        this._renderable.RenderQueueFlags = flags;
    }


    public bool RayCast(Ray ray, out HitInfo hitInfo)
    {
        hitInfo = default;
        var mesh = this.Mesh;
        if (mesh is null)
        {
            return false;
        }

        // FIXME: If the world matrix is not updated, this will not work
        Matrix4x4.Invert(this.WorldMatrix, out var invWorld);
        var localRay = Ray.Transform(ray, invWorld);

        // Fast check for bounding box intersection
        if (!localRay.Intersects(mesh.BoundingBox))
        {
            return false;
        }

        if (mesh.MeshData.RayCast(localRay, out hitInfo))
        {
            hitInfo.Position = Vector3.Transform(hitInfo.Position, this.WorldMatrix);
            hitInfo.Normal = Vector3.TransformNormal(hitInfo.Normal, this.WorldMatrix);
            hitInfo.Distance = Vector3.Distance(ray.Origin, hitInfo.Position);
            return true;
        }

        return false;
    }
}