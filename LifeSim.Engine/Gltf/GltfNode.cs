using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Gltf;

internal class GltfNode
{
    public string Name { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 Scale { get; set; }

    public Mesh? Mesh { get; set; } = null;
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
}
