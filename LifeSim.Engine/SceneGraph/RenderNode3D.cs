using System.Numerics;
using System.Runtime.InteropServices;
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
        public Vector4 AlbedoColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        public Vector4 TextureST = new Vector4(1.0f, 1.0f, 0.0f, 0.0f);
        public Vector4 HightlightColor = new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
    }

    private readonly Renderable _renderable;

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


    public Mesh? Mesh { get => this._renderable.Mesh; set => this._renderable.Mesh = value; }
    public Material? Material { get => this._renderable.Material; set => this._renderable.Material = value; }
    public Skeleton? Skeleton { get => this._renderable.Skeleton; set => this._renderable.Skeleton = value; }

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

    private bool _visible = true;
    public bool Visible
    {
        get => this._visible;
        set
        {
            if (this._visible == value) return;
            this._visible = value;
            this._renderable.Visible = value && this.Scene != null;
        }

    }

    public ShadowCasting ShadowCastingMode
    {
        get => this._renderable.ShadowCastingMode;
        set => this._renderable.ShadowCastingMode = value;
    }


    public ColorF AlbedoColor
    {
        get => this._instanceData.AlbedoColor;
        set => this.SetInstanceData(ref this._instanceData.AlbedoColor, value);
    }

    public Vector4 TextureST
    {
        get => this._instanceData.TextureST;
        set => this.SetInstanceData(ref this._instanceData.TextureST, value);
    }

    public ColorF HightlightColor
    {
        get => this._instanceData.HightlightColor;
        set => this.SetInstanceData(ref this._instanceData.HightlightColor, value);
    }

    public float Alpha
    {
        get => this._instanceData.AlbedoColor.W;
        set => this.SetInstanceData(ref this._instanceData.AlbedoColor.W, value);
    }

    protected bool SetInstanceData<T>(ref T backingField, T value) where T : unmanaged
    {
        if (backingField.Equals(value)) return false;
        backingField = value;
        this.OnInstanceDataDirty();
        return true;
    }

    private void OnInstanceDataDirty()
    {
        this._renderable.SetInstanceData(this._instanceData);
    }

    public override void UpdateTransform(ref Matrix4x4 parentMatrix)
    {
        base.UpdateTransform(ref parentMatrix);

        this._renderable.Transform = this.WorldMatrix;
    }

    internal override void AttachToSceneRecursive(Scene scene)
    {
        base.AttachToSceneRecursive(scene);
        this._renderable.Visible = this._visible;
    }

    internal override void DetachFromSceneRecursive()
    {
        if (this.Scene is null) return;
        base.DetachFromSceneRecursive();
        this._renderable.Visible = false;
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