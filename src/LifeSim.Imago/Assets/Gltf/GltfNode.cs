using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Imago.Assets.Materials;
using LifeSim.Imago.Assets.Meshes;
using LifeSim.Imago.SceneGraph.Nodes;
using LifeSim.Imago.SceneGraph.Prefabs;

namespace LifeSim.Imago.Assets.Gltf;

/// <summary>
/// Represents a node in a glTF scene graph, which can contain transformations, meshes, and child nodes.
/// </summary>
public class GltfNode : IInstantiable
{
    /// <summary>
    /// Gets or sets the name of the node.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the translation of the node relative to its parent.
    /// </summary>
    public Vector3 Position { get; set; } = Vector3.Zero;
    /// <summary>
    /// Gets or sets the rotation of the node relative to its parent.
    /// </summary>
    public Quaternion Rotation { get; set; } = Quaternion.Identity;
    /// <summary>
    /// Gets or sets the scale of the node relative to its parent.
    /// </summary>
    public Vector3 Scale { get; set; } = Vector3.One;

    /// <summary>
    /// Gets or sets the array of meshes associated with this node.
    /// </summary>
    public Mesh[] Meshes { get; set; } = Array.Empty<Mesh>();
    /// <summary>
    /// Gets or sets the skin information associated with this node, if it represents a skinned mesh.
    /// </summary>
    public GltfSkinInfo? Skin { get; set; } = null;
    /// <summary>
    /// Gets or sets the material applied to the meshes of this node.
    /// </summary>
    public Material? Material { get; set; } = null;
    /// <summary>
    /// Gets or sets the parent node in the scene graph hierarchy.
    /// </summary>
    public GltfNode? Parent { get; set; } = null;

    private readonly List<GltfNode> _children = new List<GltfNode>();
    /// <summary>
    /// Gets a read-only list of the child nodes of this node.
    /// </summary>
    public IReadOnlyList<GltfNode> Children => this._children;

    /// <summary>
    /// Initializes a new instance of the <see cref="GltfNode"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the node.</param>
    public GltfNode(string name)
    {
        this.Name = name;
    }

    /// <summary>
    /// Adds a child node to this node.
    /// </summary>
    /// <param name="node">The child <see cref="GltfNode"/> to add.</param>
    public void Add(GltfNode node)
    {
        node.Parent = this;
        this._children.Add(node);
    }

    /// <summary>
    /// Recursively searches for a node with the specified name within this node's hierarchy.
    /// </summary>
    /// <param name="name">The name of the node to find.</param>
    /// <returns>The <see cref="GltfNode"/> with the specified name, or null if not found.</returns>
    public GltfNode? FindNodeByName(string name)
    {
        // TODO: This is a naive implementation. It should be replaced with a more efficient one.
        // Like a dictionary or something.
        if (this.Name == name)
        {
            return this;
        }

        foreach (GltfNode? child in this._children)
        {
            GltfNode? result = child.FindNodeByName(name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    /// <summary>
    /// Instantiates a new <see cref="Node3D"/> scene graph from this glTF node and its children.
    /// </summary>
    /// <returns>The root <see cref="Node3D"/> of the instantiated scene graph.</returns>
    public Node3D Instantiate()
    {
        return new GltfSceneInstantiator(this).Instantiate();
    }
}
