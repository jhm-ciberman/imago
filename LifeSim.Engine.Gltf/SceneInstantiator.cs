using System.Collections.Generic;
using LifeSim.Engine.Anim;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.Resources;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Gltf;

public class SceneInstantiator
{
    private readonly Dictionary<GLTFNode, Node3D> _nodesCache = new Dictionary<GLTFNode, Node3D>();

    public SceneInstantiator()
    {
        //
    }

    internal Node3D Instantiate(GltfScene scene)
    {
        this._nodesCache.Clear();
        Node3D n = new Node3D();
        foreach (GLTFNode? node in scene.Children)
        {
            n.Add(this.InstantiateNodeRecursive(scene, node));
        }
        return n;
    }

    private Node3D CreateRenderNode(GltfScene scene, Mesh mesh, Material? material, Skin? skin)
    {
        RenderNode3D node = new RenderNode3D();
        node.Mesh = mesh;
        if (material != null)
        {
            node.Material = material;
        }
        if (skin != null)
        {
            node.Skeleton = this.CreateSkeleton(scene, skin);
        }
        return node;
    }

    private Node3D InstantiateNodeRecursive(GltfScene scene, GLTFNode gltfNode)
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

        foreach (GLTFNode? child in gltfNode.Children)
        {
            node.Add(this.InstantiateNodeRecursive(scene, child));
        }

        return node;
    }

    private Skeleton CreateSkeleton(GltfScene scene, Skin skin)
    {
        Node3D[] joints = new Node3D[skin.JointNames.Count];
        IList<string> names = skin.JointNames;
        for (var i = 0; i < names.Count; i++)
        {
            GLTFNode? gltfNode = scene.FindNodeByName(names[i]);
            joints[i] = gltfNode != null
                ? this.InstantiateNodeRecursive(scene, gltfNode)
                : throw new System.Exception("Could not bind joint: " + names[i]);
        }
        return new Skeleton(joints, skin.InverseBindMatrices);
    }
}