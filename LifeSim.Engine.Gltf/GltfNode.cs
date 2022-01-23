using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Anim;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.Resources;

namespace LifeSim.Engine.Gltf;

internal class GLTFNode
{
    public string Name;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public Mesh? Mesh = null;
    public Skin? Skin = null;
    public Material? Material = null;
    public GLTFNode? Parent = null;

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

    public string GetFullPathName()
    {
        if (this.Parent != null)
        {
            return this.Parent.GetFullPathName() + "/" + this.Name;
        }
        return "/" + this.Name;
    }
}