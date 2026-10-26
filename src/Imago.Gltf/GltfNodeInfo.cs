using System.Collections.Generic;
using System.Numerics;

namespace Imago.Gltf;

/// <summary>
/// Describes a node in a glTF file. Reading it does not load any meshes.
/// </summary>
public sealed class GltfNodeInfo
{
    /// <summary>
    /// Gets the name of the node.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the parent node, or null if this node is a scene root.
    /// </summary>
    public GltfNodeInfo? Parent { get; private set; }

    /// <summary>
    /// Gets the child nodes of this node.
    /// </summary>
    public IReadOnlyList<GltfNodeInfo> Children => this._children;

    internal Vector3 Position { get; init; } = Vector3.Zero;

    internal Quaternion Rotation { get; init; } = Quaternion.Identity;

    internal Vector3 Scale { get; init; } = Vector3.One;

    internal int? MeshIndex { get; init; }

    internal int? SkinIndex { get; init; }

    private readonly List<GltfNodeInfo> _children = new List<GltfNodeInfo>();

    internal GltfNodeInfo(string name)
    {
        this.Name = name;
    }

    /// <summary>
    /// Finds the node with the given name in this node and all its descendants.
    /// </summary>
    /// <param name="name">The name of the node to find.</param>
    /// <returns>The first node with the given name, or null if there is none.</returns>
    public GltfNodeInfo? Find(string name)
    {
        if (this.Name == name)
        {
            return this;
        }

        foreach (GltfNodeInfo child in this._children)
        {
            GltfNodeInfo? result = child.Find(name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    internal void Add(GltfNodeInfo node)
    {
        node.Parent = this;
        this._children.Add(node);
    }
}
