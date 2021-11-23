using System.Collections.Generic;
using LifeSim.Engine.Anim;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.GLTF
{
    public class SceneInstantiator
    {

        public delegate Renderable RenderableFactory();

        private readonly Dictionary<GLTFNode, Node3D> _nodesCache = new Dictionary<GLTFNode, Node3D>();
        private readonly SceneStorage _storage;

        public SceneInstantiator(SceneStorage storage)
        {
            this._storage = storage;
        }

        public Node3D Instantiate(ISceneTemplate scene)
        {
            this._nodesCache.Clear();
            Node3D n = new Node3D();
            foreach (GLTFNode? node in scene.Children)
            {
                n.Add(this._InstantiateNodeRecursive(scene, node));
            }
            return n;
        }

        private Node3D _CreateRenderNode(ISceneTemplate scene, Mesh mesh, Material? material, Skin? skin)
        {
            Renderable renderable = new Renderable(this._storage);
            renderable.SetMesh(mesh);
            if (material != null)
            {
                renderable.SetMaterial(material);
            }
            if (skin != null)
            {
                var skeleton = this._CreateSkeleton(scene, skin);
                renderable.SetSkeleton(skeleton);
            }
            return new RenderNode3D(renderable);
        }

        private Node3D _InstantiateNodeRecursive(ISceneTemplate scene, GLTFNode gltfNode)
        {
            Node3D? node3d = this._nodesCache.GetValueOrDefault(gltfNode);
            if (node3d != null)
            {
                return node3d;
            }

            Node3D node = (gltfNode.Mesh != null)
                    ? this._CreateRenderNode(scene, gltfNode.Mesh, gltfNode.Material, gltfNode.Skin)
                    : new Node3D();

            this._nodesCache[gltfNode] = node;
            node.Name = gltfNode.Name;
            node.Position = gltfNode.Position;
            node.Rotation = gltfNode.Rotation;
            node.Scale = gltfNode.Scale;

            foreach (GLTFNode? child in gltfNode.Children)
            {
                node.Add(this._InstantiateNodeRecursive(scene, child));
            }

            return node;
        }

        private Skeleton _CreateSkeleton(ISceneTemplate scene, Skin skin)
        {
            Node3D[] joints = new Node3D[skin.JointNames.Count];
            IList<string> names = skin.JointNames;
            for (var i = 0; i < names.Count; i++)
            {
                GLTFNode? gltfNode = scene.FindNodeByName(names[i]);
                joints[i] = gltfNode != null
                    ? this._InstantiateNodeRecursive(scene, gltfNode)
                    : throw new System.Exception("Could not bind joint: " + names[i]);
            }
            return new Skeleton(joints, skin.InverseBindMatrices);
        }
    }
}