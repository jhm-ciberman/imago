using System;
using System.Numerics;
using LifeSim.Imago.Graphics;
using LifeSim.Imago.Graphics.Materials;
using LifeSim.Imago.Graphics.Meshes;
using LifeSim.Imago.Graphics.Rendering;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.SceneGraph.Nodes;

/// <summary>
/// A node capable of rendering a mesh with a material.
/// </summary>
public class RenderNode3D : Node3D, IPickable
{
    private readonly Renderable _renderable;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderNode3D"/> class.
    /// </summary>
    public RenderNode3D()
    {
        this._renderable = Renderer.Instance.MakeRenderable();
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
        get => this._renderable.AlbedoColor;
        set => this._renderable.AlbedoColor = value;
    }

    /// <summary>
    /// Gets or sets the texture ST value of this node.
    /// </summary>
    public Vector4 TextureST
    {
        get => this._renderable.TextureST;
        set => this._renderable.TextureST = value;
    }

    /// <summary>
    /// Gets or sets the highlight color of this node.
    /// </summary>
    public ColorF HighlightColor
    {
        get => this._renderable.HighlightColor;
        set => this._renderable.HighlightColor = value;
    }

    /// <summary>
    /// Gets or sets the opacity of this node.
    /// </summary>
    public float Opacity
    {
        get => this._renderable.Opacity;
        set => this._renderable.Opacity = value;
    }

    /// <summary>
    /// Gets or sets the picking id of this node.
    /// </summary>
    uint IPickable.PickId
    {
        get => this._renderable.PickingId;
        set => this._renderable.PickingId = value;
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
            throw new InvalidOperationException("Cannot detach from scene if not attached to one.");

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
                this.Stage.Picking.RegisterPickable(this);
        }
        else
        {
            if (this._renderable.PickingId != 0)
                this.Stage.Picking.UnregisterPickable(this);
        }
    }

    /// <summary>
    /// Sets the mesh render info. This is a convenience method to set the mesh, material and texture ST at once.
    /// </summary>
    /// <param name="renderInfo">The render info to set.</param>
    public void SetMeshRenderInfo(MeshRenderInfo? renderInfo)
    {
        if (renderInfo is null)
        {
            this.Mesh = null;
            this.Material = null;
            return;
        }

        this.Mesh = renderInfo.Mesh;
        this.Material = renderInfo.Material;
        this.TextureST = renderInfo.TextureST;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        this._renderable?.Dispose();
    }
}
