using System;
using System.Collections.Generic;
using System.Numerics;
using Imago.Rendering.Materials;
using Imago.Rendering.Meshes;
using Imago.SceneGraph;
using Imago.SceneGraph.Prefabs;

namespace Imago.Gltf;

public class GltfNode : IInstantiable
{
    public string Name { get; set; } = string.Empty;
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Quaternion Rotation { get; set; } = Quaternion.Identity;
    public Vector3 Scale { get; set; } = Vector3.One;

    public Mesh[] Meshes { get; set; } = Array.Empty<Mesh>();
    public GltfSkinInfo? Skin { get; set; } = null;
    public Material? Material { get; set; } = null;
    public GltfNode? Parent { get; set; } = null;

    private readonly List<GltfNode> _children = new List<GltfNode>();
    public IReadOnlyList<GltfNode> Children => this._children;

    public GltfNode(string name)
    {
        this.Name = name;
    }

    public void Add(GltfNode node)
    {
        node.Parent = this;
        this._children.Add(node);
    }

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

    public Node3D Instantiate()
    {
        return new GltfSceneInstantiator(this).Instantiate();
    }
}
