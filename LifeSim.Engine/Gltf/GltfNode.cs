using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Anim;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Gltf;

public class GLTFNode
{
    public string Name { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 Scale { get; set; }

    public Mesh? Mesh { get; set; } = null;
    public Skin? Skin { get; set; } = null;
    public Material? Material { get; set; } = null;
    public GLTFNode? Parent { get; set; } = null;

    private readonly List<GLTFNode> _children = new List<GLTFNode>();
    public IReadOnlyList<GLTFNode> Children => this._children;

    public GLTFNode(string name)
    {
        this.Name = name;
    }

    public void Add(GLTFNode node)
    {
        node.Parent = this;
        this._children.Add(node);
    }
}
