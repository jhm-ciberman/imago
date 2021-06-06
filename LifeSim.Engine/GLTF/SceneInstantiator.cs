using System.Collections.Generic;
using LifeSim.Engine.Anim;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.GLTF
{
    public class SceneInstantiator
    {

        public delegate Renderable RenderableFactory();

        private Dictionary<GLTFNode, Node3D> _nodesCache = new Dictionary<GLTFNode, Node3D>();

        private SceneStorage _storage;

        public SceneInstantiator(SceneStorage storage)
        {
            this._storage = storage;
        }

        public Node3D Instantiate(ISceneTemplate scene)
        {
            this._nodesCache.Clear();
            Node3D n = new Node3D();
            foreach (GLTFNode? node in scene.children) {
                n.Add(this._InstantiateNodeRecursive(scene, node));
            }
            return n;
        }

        private Renderable _CreateRenderable(ISceneTemplate scene, Mesh mesh, SurfaceMaterial? material, Skin? skin)
        {
            Renderable renderable = new Renderable(this._storage);
            renderable.SetMesh(mesh);
            if (material != null) {
                renderable.SetMaterial(material);
            }
            if (skin != null) {
                var skeleton = this._CreateSkeleton(scene, skin);
                renderable.SetSkeleton(skeleton);
            }
            return renderable;
        }

        private Node3D _InstantiateNodeRecursive(ISceneTemplate scene, GLTFNode gltfNode)
        {
            Node3D? node3d = this._nodesCache.GetValueOrDefault(gltfNode);
            if (node3d != null) {
                return node3d;
            }

            Renderable? renderable = null;
            Node3D node = new Node3D(renderable);
            this._nodesCache[gltfNode] = node;
            node.name = gltfNode.name;
            node.position = gltfNode.position;
            node.rotation = gltfNode.rotation;
            node.scale = gltfNode.scale;
            if (gltfNode.mesh != null) {
                node.renderable = this._CreateRenderable(scene, gltfNode.mesh, gltfNode.material, gltfNode.skin);
            }
            foreach (GLTFNode? child in gltfNode.children) {
                node.Add(this._InstantiateNodeRecursive(scene, child));
            }

            return node;
        }

        private Skeleton _CreateSkeleton(ISceneTemplate scene, Skin skin)
        {
            Node3D[] joints = new Node3D[skin.jointNames.Count];
            IList<string> names = skin.jointNames;
            for (var i = 0; i < names.Count; i++) {
                GLTFNode? gltfNode = scene.FindNodeByName(names[i]);
                joints[i] = gltfNode != null
                    ? this._InstantiateNodeRecursive(scene, gltfNode)
                    : throw new System.Exception("Could not bind joint: " + names[i]);
            }
            return new Skeleton(joints, skin.inverseBindMatrices);
        }
    }
}