using System.Collections.Generic;
using Imago.Assets.Meshes;
using Imago.SceneGraph.Nodes;
using Imago.SceneGraph.Prefabs;

namespace Imago.Assets.Wavefront;

/// <summary>
/// Represents a node in a Wavefront OBJ file that can contain multiple mesh groups.
/// </summary>
public class ObjNode : IInstantiable
{
    /// <summary>
    /// Gets or sets the name of the OBJ node.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the collection of mesh groups contained in this node, indexed by group name.
    /// </summary>
    public Dictionary<string, Mesh> Groups { get; } = new Dictionary<string, Mesh>();

    /// <summary>
    /// Creates a 3D scene node by instantiating all mesh groups as child render nodes.
    /// </summary>
    /// <returns>A new 3D node containing all the mesh groups as children.</returns>
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

    /// <summary>
    /// Finds a mesh group by name.
    /// </summary>
    /// <param name="name">The name of the group to find.</param>
    /// <returns>The mesh if found; otherwise, null.</returns>
    public Mesh? FindGroup(string name)
    {
        if (this.Groups.TryGetValue(name, out var mesh))
        {
            return mesh;
        }

        return null;
    }
}
