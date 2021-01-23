using System.Collections.Generic;
using LifeSim.Engine.Anim;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.GLTF
{
    public class GLTFScene
    {
        private List<GLTFNode> children = new List<GLTFNode>();
        private Dictionary<string, GLTFNode> _nodesByName = new Dictionary<string, GLTFNode>();

        public readonly string name;

        public GLTFScene(string name)
        {
            this.name = name;
        }

        private BindedSkin _BindSkin(Skin skin, Dictionary<GLTFNode, Node3D> nodesCache)
        {
            Node3D[] joints = new Node3D[skin.jointNames.Count];
            var names = skin.jointNames;
            for (int i = 0; i < names.Count; i++) {
                var gltfNode = this._nodesByName.GetValueOrDefault(names[i]);
                if (gltfNode != null) {
                    joints[i] = this._InstantiateNodeRecursive(gltfNode, nodesCache);
                } else {
                    throw new System.Exception("Could not bind joint: " + names[i]);
                }
            }

            return new BindedSkin(joints, skin.inverseBindMatrices);
        }

        public void Add(GLTFNode node)
        {
            this.children.Add(node);
            this._AddToDictionaryRecursive(this.name, node);
        } 

        private void _AddToDictionaryRecursive(string currentPath, GLTFNode node)
        {
            //currentPath += "/" + node.name;
            currentPath = node.name;
            this._nodesByName[currentPath] = node;
            foreach (var child in node.children) {
                this._AddToDictionaryRecursive(currentPath, child);
            }
        }

        public Node3D Instantiate()
        {
            var nodesCache = new Dictionary<GLTFNode, Node3D>();

            var n = new Node3D();
            foreach (var node in this.children) {
                n.Add(this._InstantiateNodeRecursive(node, nodesCache));
            }
            return n;
        }

        private Node3D _InstantiateNodeRecursive(GLTFNode gltfNode, Dictionary<GLTFNode, Node3D> nodesCache)
        {
            var node3d = nodesCache.GetValueOrDefault(gltfNode);
            if (node3d != null) return node3d;

            Node3D node;
            if (gltfNode.mesh != null && gltfNode.material != null) {
                if (gltfNode.skin != null) {
                    var bindedSkin = this._BindSkin(gltfNode.skin, nodesCache);
                    node = new SkinnedRenderable3D(gltfNode.mesh, gltfNode.material, bindedSkin);
                } else {
                    node = new Renderable3D(gltfNode.mesh, gltfNode.material);
                }
            } else {
                node = new Node3D();
            }
            nodesCache[gltfNode] = node;
            node.name = gltfNode.name;
            node.position = gltfNode.position;
            node.rotation = gltfNode.rotation;
            node.scale = gltfNode.scale;
            foreach (var child in gltfNode.children) {
                node.Add(this._InstantiateNodeRecursive(child, nodesCache));
            }

            return node;
        }

    }
}