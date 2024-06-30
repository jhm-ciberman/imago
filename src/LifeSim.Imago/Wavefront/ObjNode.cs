using System.Collections.Generic;
using LifeSim.Imago.Graphics.Meshes;
using LifeSim.Imago.SceneGraph.Nodes;
using LifeSim.Imago.SceneGraph.Prefabs;

namespace LifeSim.Imago.Wavefront;

public class ObjNode : IInstantiable
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, Mesh> Meshes { get; } = new Dictionary<string, Mesh>();
    public ObjNode? Parent { get; private set; }
    private readonly List<ObjNode> _children = new List<ObjNode>();
    public IReadOnlyList<ObjNode> Children => _children;

    public void AddChild(ObjNode child)
    {
        child.Parent = this;
        this._children.Add(child);
    }

    public Node3D Instantiate()
    {
        var node = new Node3D { Name = this.Name };

        foreach (var mesh in this.Meshes)
        {
            var renderNode = new RenderNode3D { Name = mesh.Key, Mesh = mesh.Value };
            node.AddChild(renderNode);
        }

        foreach (var child in this.Children)
        {
            node.AddChild(child.Instantiate());
        }

        return node;
    }
}
