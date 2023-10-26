using System.Collections.Generic;

namespace Imago.SceneGraph;


public class MultiMeshRenderNode3D : Node3D
{
    private readonly List<RenderNode3D> _renderNodes = new List<RenderNode3D>();

    private MaterialMesh[] _meshes = System.Array.Empty<MaterialMesh>();

    public MultiMeshRenderNode3D()
    {
    }

    protected virtual RenderNode3D CreateRenderNode()
    {
        return new RenderNode3D(); // Override to use a different type of render node.
    }

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

    public MaterialMesh[] Meshes
    {
        get => this._meshes;
        set => this.SetMeshes(value);
    }

    public bool AutoDisposeMeshes { get; set; } = true;

    public bool AutoDisposeMaterials { get; set; } = false;

    public IReadOnlyList<RenderNode3D> ChildRenderNodes => this._renderNodes;

    private void SetMeshes(MaterialMesh[] meshes)
    {
        this._meshes = meshes;

        this.EnsureRenderNodesCount(meshes.Length);

        for (int i = 0; i < meshes.Length; i++)
        {
            this._renderNodes[i].Mesh = meshes[i].Mesh;
            this._renderNodes[i].Material = meshes[i].Material;
            this._renderNodes[i].TextureST = meshes[i].TextureST;
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

    override protected void Dispose(bool disposing)
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
