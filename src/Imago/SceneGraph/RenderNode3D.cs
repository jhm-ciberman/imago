using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Imago.Rendering;
using Imago.Rendering.Materials;
using Support;
using Veldrid.Utilities;

namespace Imago.SceneGraph;

/// <summary>
/// A node capable of rendering a mesh with a material.
/// </summary>
public class RenderNode3D : Node3D, IPickable
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

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderNode3D"/> class.
    /// </summary>
    public RenderNode3D()
    {
        this._instanceData = new InstanceData();

        this._renderable = Renderer.Instance.MakeRenderable<InstanceData>();
        this._renderable.SetInstanceData(this._instanceData);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderNode3D"/> class.
    /// </summary>
    /// <param name="mesh">The mesh to render.</param>
    public RenderNode3D(Mesh mesh) : this()
    {
        this.Mesh = mesh;
    }

    /// <summary>
    /// Gets or sets the mesh to render.
    /// </summary>
    public Mesh? Mesh
    {
        get => this._renderable.Mesh;
        set => this._renderable.Mesh = value;
    }

    /// <summary>
    /// Gets or sets the material to use.
    /// </summary>
    public Material? Material
    {
        get => this._renderable.Material;
        set => this._renderable.Material = value;
    }

    /// <summary>
    /// Gets or sets the skeleton to use.
    /// </summary>
    public Skeleton? Skeleton
    {
        get => this._renderable.Skeleton;
        set => this._renderable.Skeleton = value;
    }

    private bool _isPickable = false;

    /// <summary>
    /// Gets or sets whether this node is pickable by the user using the mouse.
    /// </summary>
    public bool IsPickable
    {
        get => this._isPickable;
        set
        {
            if (this._isPickable == value) return;
            this._isPickable = value;
            this.UpdateRegistrationWithPickingManager();
        }
    }

    private bool _visible = true;

    /// <summary>
    /// Gets or sets whether this node is visible.
    /// </summary>
    public bool Visible
    {
        get => this._visible;
        set
        {
            if (this._visible == value) return;
            this._visible = value;
            this._renderable.Visible = value && this.Stage != null;
        }

    }

    /// <summary>
    /// Gets or sets the shadow cast mode.
    /// </summary>
    public ShadowCasting ShadowCastingMode
    {
        get => this._renderable.ShadowCastingMode;
        set => this._renderable.ShadowCastingMode = value;
    }

    /// <summary>
    /// Gets or sets the albedo color of this node.
    /// </summary>
    public ColorF AlbedoColor
    {
        get => this._instanceData.AlbedoColor;
        set => this.SetInstanceData(ref this._instanceData.AlbedoColor, value);
    }

    /// <summary>
    /// Gets or sets the texture ST value of this node.
    /// </summary>
    public Vector4 TextureST
    {
        get => this._instanceData.TextureST;
        set => this.SetInstanceData(ref this._instanceData.TextureST, value);
    }

    /// <summary>
    /// Gets or sets the highlight color of this node.
    /// </summary>
    public ColorF HightlightColor
    {
        get => this._instanceData.HightlightColor;
        set => this.SetInstanceData(ref this._instanceData.HightlightColor, value);
    }

    /// <summary>
    /// Gets or sets the opacity of this node.
    /// </summary>
    public float Opacity
    {
        get => this._instanceData.AlbedoColor.W;
        set
        {
            if (this.SetInstanceData(ref this._instanceData.AlbedoColor.W, value))
            {
                this._renderable.Transparent = value < 1.0f;
            }
        }
    }

    /// <summary>
    /// Gets or sets the picking id of this node.
    /// </summary>
    uint IPickable.PickId
    {
        get => this._renderable.PickingId;
        set => this._renderable.PickingId = value;
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

    internal override void AttachToStage(Stage stage)
    {
        base.AttachToStage(stage);
        this.UpdateRegistrationWithPickingManager();
        this._renderable.Visible = this._visible;
        this._renderable.Stage = stage;
    }

    internal override void DetachFromStage()
    {
        if (this.Stage is null)
        {
            throw new InvalidOperationException("Cannot detach from scene if not attached to one.");
        }

        this._renderable.Visible = false;
        this._renderable.Stage = null;
        this.UpdateRegistrationWithPickingManager();
        base.DetachFromStage();
    }

    private void UpdateRegistrationWithPickingManager()
    {
        if (this.Stage is null) return;

        if (this._isPickable)
        {
            if (this._renderable.PickingId == 0)
            {
                this.Stage.Picking.RegisterPickable(this);
            }
        }
        else
        {
            if (this._renderable.PickingId != 0)
            {
                this.Stage.Picking.UnregisterPickable(this);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        this._renderable?.Dispose();
    }
}
