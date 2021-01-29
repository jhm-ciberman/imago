using System.Collections.Generic;
using LifeSim.Engine.Anim;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.GLTF
{
    public class GLTFScene
    {
        private readonly List<GLTFNode> _children = new List<GLTFNode>();
        private readonly Dictionary<string, GLTFNode> _nodesByName = new Dictionary<string, GLTFNode>();

        public readonly string name;

        public GLTFScene(string name)
        {
            this.name = name;
        }

        private BindedSkin _BindSkin(Skin skin, Dictionary<GLTFNode, Node3D> nodesCache)
        {
            Node3D[] joints = new Node3D[skin.jointNames.Count];
            IList<string>? names = skin.jointNames;
            for (var i = 0; i < names.Count; i++) {
                GLTFNode? gltfNode = this._nodesByName.GetValueOrDefault(names[i]);
                joints[i] = gltfNode != null
                    ? this._InstantiateNodeRecursive(gltfNode, nodesCache)
                    : throw new System.Exception("Could not bind joint: " + names[i]);
            }

            return new BindedSkin(joints, skin.inverseBindMatrices);
        }

        public void Add(GLTFNode node)
        {
            this._children.Add(node);
            this._AddToDictionaryRecursive(node);
        }

        private void _AddToDictionaryRecursive(GLTFNode node)
        {
            this._nodesByName[node.name] = node;
            foreach (GLTFNode? child in node.children) {
                this._AddToDictionaryRecursive(child);
            }
        }

        public Node3D Instantiate()
        {
            Dictionary<GLTFNode, Node3D>? nodesCache = new Dictionary<GLTFNode, Node3D>();

            Node3D? n = new Node3D();
            foreach (GLTFNode? node in this._children) {
                n.Add(this._InstantiateNodeRecursive(node, nodesCache));
            }
            return n;
        }

        private Node3D _InstantiateNodeRecursive(GLTFNode gltfNode, Dictionary<GLTFNode, Node3D> nodesCache)
        {
            Node3D? node3d = nodesCache.GetValueOrDefault(gltfNode);
            if (node3d != null) {
                return node3d;
            }

            Node3D node;
            if (gltfNode.mesh != null && gltfNode.material != null) {
                if (gltfNode.skin != null) {
                    BindedSkin? bindedSkin = this._BindSkin(gltfNode.skin, nodesCache);
                    node = new SkinnedRenderable3D(gltfNode.mesh, gltfNode.material, bindedSkin);
                }
                else {
                    node = new Renderable3D(gltfNode.mesh, gltfNode.material);
                }
            }
            else {
                node = new Node3D();
            }
            nodesCache[gltfNode] = node;
            node.name = gltfNode.name;
            node.position = gltfNode.position;
            node.rotation = gltfNode.rotation;
            node.scale = gltfNode.scale;
            foreach (GLTFNode? child in gltfNode.children) {
                node.Add(this._InstantiateNodeRecursive(child, nodesCache));
            }

            return node;
        }

    }
}