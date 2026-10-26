using System;
using System.Collections.Generic;
using Imago.Graphics3D.Meshes;
using Imago.Graphics3D.SceneGraph;
using Imago.Graphics3D.SceneGraph.Nodes;

namespace Imago.Gltf;

/// <summary>
/// Responsible for instantiating a glTF scene graph into a hierarchy of <see cref="Node3D"/> objects.
/// </summary>
internal class GltfSceneInstantiator
{
    private readonly Dictionary<GltfNodeInfo, Node3D> _nodesCache = new Dictionary<GltfNodeInfo, Node3D>();

    private readonly GltfNodeInfo _node;
    private readonly IReadOnlyDictionary<int, Mesh[]> _meshes;
    private readonly IReadOnlyDictionary<int, GltfSkinInfo> _skins;

    /// <summary>
    /// Initializes a new instance of the <see cref="GltfSceneInstantiator"/> class.
    /// </summary>
    /// <param name="node">The root <see cref="GltfNodeInfo"/> to instantiate.</param>
    /// <param name="meshes">The meshes of the tree, by glTF mesh index.</param>
    /// <param name="skins">The skins of the tree, by glTF skin index.</param>
    internal GltfSceneInstantiator(GltfNodeInfo node, IReadOnlyDictionary<int, Mesh[]> meshes, IReadOnlyDictionary<int, GltfSkinInfo> skins)
    {
        this._node = node;
        this._meshes = meshes;
        this._skins = skins;
    }

    /// <summary>
    /// Instantiates the glTF scene graph into a hierarchy of <see cref="Node3D"/> objects.
    /// </summary>
    /// <returns>The root <see cref="Node3D"/> of the instantiated scene graph.</returns>
    internal Node3D Instantiate()
    {
        return this.InstantiateNodeRecursive(this._node);
    }

    private RenderNode3D CreateRenderNode(Mesh mesh, GltfSkinInfo? skin)
    {
        return new RenderNode3D()
        {
            Mesh = mesh,
            Skeleton = (skin != null) ? this.CreateSkeleton(skin) : null,
        };
    }

    private Node3D InstantiateNodeRecursive(GltfNodeInfo gltfNode)
    {
        Node3D? node3d = this._nodesCache.GetValueOrDefault(gltfNode);
        if (node3d != null)
        {
            return node3d;
        }

        var meshes = gltfNode.MeshIndex is int meshIndex ? this._meshes[meshIndex] : Array.Empty<Mesh>();
        var skin = gltfNode.SkinIndex is int skinIndex ? this._skins[skinIndex] : null;
        Node3D node;

        if (meshes.Length == 0)
        {
            node = new Node3D();
        }
        else if (meshes.Length == 1)
        {
            node = this.CreateRenderNode(meshes[0], skin);
        }
        else // Multi primitive mesh. Our engine does not support this, so we create a node for each primitive. KISS.
        {
            node = new Node3D();
            for (var i = 0; i < meshes.Length; i++)
            {
                var childNode = this.CreateRenderNode(meshes[i], skin);
                childNode.Name = $"{gltfNode.Name}_{i}";
                node.AddChild(childNode);
            }
        }

        this._nodesCache[gltfNode] = node;
        node.Name = gltfNode.Name;
        node.Position = gltfNode.Position;
        node.Rotation = gltfNode.Rotation;
        node.Scale = gltfNode.Scale;

        foreach (GltfNodeInfo child in gltfNode.Children)
        {
            node.AddChild(this.InstantiateNodeRecursive(child));
        }

        return node;
    }

    private Skeleton CreateSkeleton(GltfSkinInfo skin)
    {
        Node3D[] joints = new Node3D[skin.JointNames.Count];
        IList<string> names = skin.JointNames;

        var nodesByName = new Dictionary<string, GltfNodeInfo>();
        PopulateNodesDictionary(this._node, nodesByName);

        for (var i = 0; i < names.Count; i++)
        {
            GltfNodeInfo? gltfNode = nodesByName.GetValueOrDefault(names[i]);
            joints[i] = gltfNode != null
                ? this.InstantiateNodeRecursive(gltfNode)
                : throw new InvalidOperationException($"Could not bind joint: {names[i]}");
        }
        return new Skeleton(joints, skin.InverseBindMatrices);
    }

    private static void PopulateNodesDictionary(GltfNodeInfo node, Dictionary<string, GltfNodeInfo> nodes)
    {
        nodes[node.Name] = node;

        foreach (GltfNodeInfo child in node.Children)
        {
            PopulateNodesDictionary(child, nodes);
        }
    }
}
