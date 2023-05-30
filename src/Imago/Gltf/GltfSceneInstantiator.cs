using System;
using System.Collections.Generic;
using Imago.Rendering;
using Imago.SceneGraph;

namespace Imago.Gltf;

internal class GltfSceneInstantiator
{
    private readonly Dictionary<GltfNode, Node3D> _nodesCache = new Dictionary<GltfNode, Node3D>();

    private readonly GltfNode _node;

    internal GltfSceneInstantiator(GltfNode node)
    {
        this._node = node;
    }

    internal Node3D Instantiate()
    {
        return this.InstantiateNodeRecursive(this._node);
    }

    private Node3D CreateRenderNode(Mesh mesh, Material? material, GltfSkinInfo? skin)
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

        Node3D node = (gltfNode.Mesh != null)
            ? this.CreateRenderNode(gltfNode.Mesh, gltfNode.Material, gltfNode.Skin)
            : new Node3D();

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
