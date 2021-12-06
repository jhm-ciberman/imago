using System.Collections.Generic;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.GLTF
{
    public partial class GLTFScene
    {
        private readonly List<GLTFNode> _children = new List<GLTFNode>();
        private readonly Dictionary<string, GLTFNode> _nodesByName = new Dictionary<string, GLTFNode>();
        public readonly string Name;

        public GLTFScene(string name)
        {
            this.Name = name;
        }

        public IReadOnlyList<GLTFNode> Children => this._children;

        public GLTFNode? FindNodeByName(string name)
        {
            return this._nodesByName.GetValueOrDefault(name);
        }

        public void Add(GLTFNode node)
        {
            this._children.Add(node);
            this._AddToDictionaryRecursive(node);
        }

        private void _AddToDictionaryRecursive(GLTFNode node)
        {
            this._nodesByName[node.Name] = node;
            foreach (GLTFNode? child in node.Children)
            {
                this._AddToDictionaryRecursive(child);
            }
        }

        public Node3D Instantiate(SceneStorage storage)
        {
            return new SceneInstantiator(storage).Instantiate(this);
        }
    }
}