using System.Collections.Generic;
using LifeSim.Imago.Meshes;
using LifeSim.Imago.SceneGraph.Nodes;
using LifeSim.Imago.SceneGraph.Prefabs;

namespace LifeSim.Imago.Wavefront;

public class ObjNode : IInstantiable
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, Mesh> Groups { get; } = new Dictionary<string, Mesh>();

    public Node3D Instantiate()
    {
        var node = new Node3D { Name = this.Name };

        foreach (var mesh in this.Groups)
        {
            var renderNode = new RenderNode3D { Name = mesh.Key, Mesh = mesh.Value };
            node.AddChild(renderNode);
        }

        return node;
    }

    public Mesh? FindGroup(string name)
    {
        if (this.Groups.TryGetValue(name, out var mesh))
        {
            return mesh;
        }

        return null;
    }
}
