using System.Collections.Generic;
using LifeSim.Core;

namespace LifeSim.Engine.SceneGraph
{
    public class Scene3D
    {
        public DirectionalLight mainLight = new DirectionalLight();
        
        public ColorF ambientColor = new ColorF(.2f, .2f, .2f);

        public ColorF clearColor = new ColorF(0.84f, 0.84f, 0.86f, 1.0f);
        //private RgbaFloat _clearColor = new RgbaFloat(0.04f, 0.04f, 0.06f, 1.0f);

        private SwapPopList<RenderNode3D> _renderables = new SwapPopList<RenderNode3D>();
        public IReadOnlyList<RenderNode3D> renderables => this._renderables;

        private List<Node3D> _dirtyList = new List<Node3D>();

        private readonly SwapPopList<Node3D> _children = new SwapPopList<Node3D>();
        public IReadOnlyList<Node3D> children => this._children;

        public Scene3D()
        {
            //
        }

        public void Add(Node3D node)
        {
            this._children.Add(node);
            this._OnChildAdded(node);
        }

        public void Remove(Node3D node)
        {
            this._children.Remove(node);
            this._OnChildRemoved(node);
        }

        internal void _OnChildAdded(Node3D node)
        {
            this._OnTransformDirty(node);
            this._AddNodeToRecursive(node);
        }

        internal void _OnChildRemoved(Node3D node)
        {
            this._OnTransformDirty(node);
            this._RemoveNodeRecursive(node);
        }

        internal void _OnTransformDirty(Node3D node)
        {
            this._dirtyList.Add(node);
        }

        private void _AddNodeToRecursive(Node3D node)
        {
            node._scene = this;
            if (node is RenderNode3D renderable) {
                this._renderables.Add(renderable);
            }
            for (int i = 0; i < node.children.Count; i++) {
                this._AddNodeToRecursive(node.children[i]);
            }
        }

        private void _RemoveNodeRecursive(Node3D node)
        {
            node._scene = null;
            if (node is RenderNode3D renderable) {
                this._renderables.Remove(renderable);
            }
            for (int i = 0; i < node.children.Count; i++) {
                this._RemoveNodeRecursive(node.children[i]);
            }
        }

        public void UpdateWorldMatrices()
        {
            if (this._dirtyList.Count > 0) {
                for (int i = 0; i < this._dirtyList.Count; i++) {
                    var dirtyNode = this._dirtyList[i];
                    if (! dirtyNode.transformIsDirty) continue;
                    this._SearchTopDirty(dirtyNode).UpdateWorldMatrix();
                }
                this._dirtyList.Clear();
            }
        }

        private Node3D _SearchTopDirty(Node3D node)
        {
            Node3D topDirty = node;
            while (true) {
                if (node.transformIsDirty) topDirty = node;
                if (node.parent == null) return topDirty;
                node = node.parent;
            }
        }
    }
}