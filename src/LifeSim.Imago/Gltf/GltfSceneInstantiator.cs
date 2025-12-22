using System;
using System.Collections.Generic;
using LifeSim.Imago.Materials;
using LifeSim.Imago.Meshes;
using LifeSim.Imago.SceneGraph;
using LifeSim.Imago.SceneGraph.Nodes;

namespace LifeSim.Imago.Gltf;

/// <summary>
/// Responsible for instantiating a glTF scene graph into a hierarchy of <see cref="Node3D"/> objects.
/// </summary>
internal class GltfSceneInstantiator
{
    private readonly Dictionary<GltfNode, Node3D> _nodesCache = new Dictionary<GltfNode, Node3D>();

    private readonly GltfNode _node;

    /// <summary>
    /// Initializes a new instance of the <see cref="GltfSceneInstantiator"/> class.
    /// </summary>
    /// <param name="node">The root <see cref="GltfNode"/> to instantiate.</param>
    internal GltfSceneInstantiator(GltfNode node)
    {
        this._node = node;
    }

    /// <summary>
    /// Instantiates the glTF scene graph into a hierarchy of <see cref="Node3D"/> objects.
    /// </summary>
    /// <returns>The root <see cref="Node3D"/> of the instantiated scene graph.</returns>
    internal Node3D Instantiate()
    {
        return this.InstantiateNodeRecursive(this._node);
    }

    private RenderNode3D CreateRenderNode(Mesh mesh, Material? material, GltfSkinInfo? skin)
    {
        return new RenderNode3D()
        {
            Mesh = mesh,
            Material = material,
            Skeleton = (skin != null) ? this.CreateSkeleton(skin) : null,
        };
    }

    private Node3D InstantiateNodeRecursive(GltfNode gltfNode)
    {
        Node3D? node3d = this._nodesCache.GetValueOrDefault(gltfNode);
        if (node3d != null)
        {
            return node3d;
        }

        var meshes = gltfNode.Meshes;
        Node3D node;

        if (meshes.Length == 0)
        {
            node = new Node3D();
        }
        else if (meshes.Length == 1)
        {
            node = this.CreateRenderNode(meshes[0], gltfNode.Material, gltfNode.Skin);
        }
        else // Multi primitive mesh. Our engine do not LifeSim.Support this so we create a node for each primitive. KISS.
        {
            node = new Node3D();
            for (var i = 0; i < meshes.Length; i++)
            {
                var childNode = this.CreateRenderNode(meshes[i], gltfNode.Material, gltfNode.Skin);
                childNode.Name = $"{gltfNode.Name}_{i}";
                node.AddChild(childNode);
            }
        }

        this._nodesCache[gltfNode] = node;
        node.Name = gltfNode.Name;
        node.Position = gltfNode.Position;
        node.Rotation = gltfNode.Rotation;
        node.Scale = gltfNode.Scale;

        foreach (GltfNode? child in gltfNode.Children)
        {
            node.AddChild(this.InstantiateNodeRecursive(child));
        }

        return node;
    }

    private Skeleton CreateSkeleton(GltfSkinInfo skin)
    {
        Node3D[] joints = new Node3D[skin.JointNames.Count];
        IList<string> names = skin.JointNames;

        var nodesByName = new Dictionary<string, GltfNode>();
        PopulateNodesDictionary(this._node, nodesByName);

        for (var i = 0; i < names.Count; i++)
        {
            GltfNode? gltfNode = nodesByName.GetValueOrDefault(names[i]);
            joints[i] = gltfNode != null
                ? this.InstantiateNodeRecursive(gltfNode)
                : throw new InvalidOperationException($"Could not bind joint: {names[i]}");
        }
        return new Skeleton(joints, skin.InverseBindMatrices);
    }

    private static void PopulateNodesDictionary(GltfNode node, Dictionary<string, GltfNode> nodes)
    {
        nodes[node.Name] = node;

        foreach (GltfNode? child in node.Children)
        {
            PopulateNodesDictionary(child, nodes);
        }
    }
}
