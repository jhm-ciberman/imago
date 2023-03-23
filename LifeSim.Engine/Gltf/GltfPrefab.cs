using System;
using System.Collections.Generic;
using LifeSim.Engine.Assets;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Gltf;

public class GltfPrefab : Prefab
{
    private readonly List<GltfNode> _children = new List<GltfNode>();
    private readonly Dictionary<string, GltfNode> _nodesByName = new Dictionary<string, GltfNode>();

    public GltfPrefab(string name)
    {
        this.Name = name;
    }

    internal GltfNode? FindNodeByName(string name)
    {
        return this._nodesByName.GetValueOrDefault(name);
    }

    internal void Add(GltfNode node)
    {
        this._children.Add(node);
        this.AddToDictionaryRecursive(node);
    }

    private void AddToDictionaryRecursive(GltfNode node)
    {
        this._nodesByName[node.Name] = node;
        foreach (GltfNode? child in node.Children)
        {
            this.AddToDictionaryRecursive(child);
        }
    }

    protected override Node3D InstantiateCore()
    {
        return new SceneInstantiator().Instantiate(this);
    }

    private class SceneInstantiator
    {
        private readonly Dictionary<GltfNode, Node3D> _nodesCache = new Dictionary<GltfNode, Node3D>();

        public SceneInstantiator()
        {
            //
        }

        internal Node3D Instantiate(GltfPrefab scene)
        {
            this._nodesCache.Clear();
            Node3D node = new Node3D();
            foreach (GltfNode? gltfNode in scene._children)
            {
                node.AddChild(this.InstantiateNodeRecursive(scene, gltfNode));
            }
            return node;
        }

        private Node3D CreateRenderNode(GltfPrefab scene, Mesh mesh, Material? material, GltfSkinInfo? skin)
        {
            return new RenderNode3D()
            {
                Mesh = mesh,
                Material = material,
                Skeleton = (skin != null) ? this.CreateSkeleton(scene, skin) : null,
            };
        }

        private Node3D InstantiateNodeRecursive(GltfPrefab scene, GltfNode gltfNode)
        {
            Node3D? node3d = this._nodesCache.GetValueOrDefault(gltfNode);
            if (node3d != null)
            {
                return node3d;
            }

            Node3D node = (gltfNode.Mesh != null)
                ? this.CreateRenderNode(scene, gltfNode.Mesh, gltfNode.Material, gltfNode.Skin)
                : new Node3D();

            this._nodesCache[gltfNode] = node;
            node.Name = gltfNode.Name;
            node.Position = gltfNode.Position;
            node.Rotation = gltfNode.Rotation;
            node.Scale = gltfNode.Scale;


            foreach (GltfNode? child in gltfNode.Children)
            {
                node.AddChild(this.InstantiateNodeRecursive(scene, child));
            }

            return node;
        }

        private Skeleton CreateSkeleton(GltfPrefab scene, GltfSkinInfo skin)
        {
            Node3D[] joints = new Node3D[skin.JointNames.Count];
            IList<string> names = skin.JointNames;
            for (var i = 0; i < names.Count; i++)
            {
                GltfNode? gltfNode = scene.FindNodeByName(names[i]);
                joints[i] = gltfNode != null
                    ? this.InstantiateNodeRecursive(scene, gltfNode)
                    : throw new InvalidOperationException($"Could not bind joint: {names[i]}");
            }
            return new Skeleton(joints, skin.InverseBindMatrices);
        }
    }

}
