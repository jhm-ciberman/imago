using System.Collections.Generic;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.SceneGraph.Nodes;


public class MultiMeshRenderNode3D : Node3D
{
    private readonly List<RenderNode3D> _renderNodes = new List<RenderNode3D>();

    private MeshRenderInfo[] _meshes = System.Array.Empty<MeshRenderInfo>();

    /// <summary>
    /// Creates an instance of the <see cref="MultiMeshRenderNode3D"/> class.
    /// </summary>
    public MultiMeshRenderNode3D()
    {
    }

    /// <summary>
    /// Creates a new render node. Override this method to use a different type of render node.
    /// </summary>
    /// <returns>The created render node.</returns>
    protected virtual RenderNode3D CreateRenderNode()
    {
        return new RenderNode3D();
    }

    /// <summary>
    /// Disposes a render node. Override this method to dispose the render node in a different way.
    /// </summary>
    /// <param name="node">The render node to dispose.</param>
    protected virtual void DisposeRenderNode(RenderNode3D node)
    {
        if (this.AutoDisposeMeshes)
        {
            node.Mesh?.Dispose();
            node.Mesh = null;
        }

        if (this.AutoDisposeMaterials)
        {
            node.Material?.Dispose();
            node.Material = null;
        }

        node.Dispose();
    }

    /// <summary>
    /// Gets or sets the meshes to render.
    /// </summary>
    /// <remarks>
    /// To update the meshes, set this property to a new array of meshes. If you change the
    /// contents of the array, you must also set this property to the same array again to trigger
    /// an update of the render nodes.
    /// </remarks>
    public MeshRenderInfo[] Meshes
    {
        get => this._meshes;
        set => this.SetMeshes(value);
    }

    /// <summary>
    /// Gets or sets whether the meshes should be disposed when the node is disposed.
    /// </summary>
    public bool AutoDisposeMeshes { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the materials should be disposed when the node is disposed.
    /// </summary>
    public bool AutoDisposeMaterials { get; set; } = false;

    /// <summary>
    /// Gets the list of render nodes.
    /// </summary>
    public IReadOnlyList<RenderNode3D> ChildRenderNodes => this._renderNodes;

    private bool _isPickable = false;

    /// <summary>
    /// Gets or sets whether the node is pickable.
    /// </summary>
    public bool IsPickable
    {
        get => this._isPickable;
        set
        {
            if (this._isPickable == value) return;
            this._isPickable = value;
            foreach (var node in this._renderNodes)
            {
                node.IsPickable = value;
            }
        }
    }

    private ColorF _albedoColor = ColorF.White;

    /// <summary>
    /// Gets or sets the albedo color of the meshes.
    /// </summary>
    public ColorF AlbedoColor
    {
        get => this._albedoColor;
        set
        {
            if (this._albedoColor == value) return;
            this._albedoColor = value;
            foreach (var node in this._renderNodes)
            {
                node.AlbedoColor = value;
            }
        }
    }

    private ColorF _highlightColor = new ColorF(1.0f, 1.0f, 1.0f, 0.0f);

    /// <summary>
    /// Gets or sets the highlight color of the meshes.
    /// </summary>
    public ColorF HighlightColor
    {
        get => this._highlightColor;
        set
        {
            if (this._highlightColor == value) return;
            this._highlightColor = value;
            foreach (var node in this._renderNodes)
            {
                node.HighlightColor = value;
            }
        }
    }

    private void SetMeshes(MeshRenderInfo[] meshes)
    {
        this._meshes = meshes;

        this.EnsureRenderNodesCount(meshes.Length);

        for (int i = 0; i < meshes.Length; i++)
        {
            this._renderNodes[i].Mesh = meshes[i].Mesh;
            this._renderNodes[i].Material = meshes[i].Material;
            this._renderNodes[i].TextureST = meshes[i].TextureST;
            this._renderNodes[i].AlbedoColor = this.AlbedoColor;
            this._renderNodes[i].HighlightColor = this.HighlightColor;
        }
    }

    private void EnsureRenderNodesCount(int neededCount)
    {
        var actualCount = this._renderNodes.Count;
        if (actualCount < neededCount)
        {
            for (int i = actualCount; i < neededCount; i++)
            {
                var node = this.CreateRenderNode();
                node.IsPickable = this.IsPickable;
                this.AddChild(node);
                this._renderNodes.Add(node);
            }
        }
        else if (actualCount > neededCount)
        {
            for (int i = neededCount; i < actualCount; i++)
            {
                var node = this._renderNodes[i];
                this.RemoveChild(node, dispose: false);
                this.DisposeRenderNode(node);
            }

            this._renderNodes.RemoveRange(neededCount, actualCount - neededCount);
        }

        this._renderNodes.TrimExcess();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var node in this._renderNodes)
            {
                this.DisposeRenderNode(node);
            }
        }

        base.Dispose(disposing);
    }
}
