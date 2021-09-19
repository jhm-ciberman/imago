using System.Collections.Generic;
using LifeSim.Core;
using LifeSim.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class Scene3D
    {
        public DirectionalLight MainLight = new DirectionalLight();
        
        public ColorF AmbientColor = new ColorF(.2f, .2f, .2f);

        public ColorF ClearColor = new ColorF(0.84f, 0.84f, 0.86f, 1.0f);
        //private RgbaFloat _clearColor = new RgbaFloat(0.04f, 0.04f, 0.06f, 1.0f);

        private readonly SwapPopList<Node3D> _children = new SwapPopList<Node3D>();
        public IReadOnlyList<Node3D> Children => this._children;

        private readonly SwapPopList<Renderable> _renderables = new SwapPopList<Renderable>();

        public IReadOnlyList<Renderable> Renderables => this._renderables;

        private readonly List<Node3D> _transformDirtyList = new List<Node3D>();

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
            this._transformDirtyList.Add(node);
            this._AddNodeToRecursive(node);
        }

        internal void _OnChildRemoved(Node3D node)
        {
            this._transformDirtyList.Add(node);
            this._RemoveNodeRecursive(node);
        }

        internal void _OnTransformDirty(Node3D node)
        {
            this._transformDirtyList.Add(node);
        }


        private void _AddNodeToRecursive(Node3D node)
        {
            node.Scene = this;
            if (node.Renderable != null) {
                this.AddRenderable(node.Renderable);
            }
            for (int i = 0; i < node.Children.Count; i++) {
                this._AddNodeToRecursive(node.Children[i]);
            }
        }

        private void _RemoveNodeRecursive(Node3D node)
        {
            node.Scene = null;
            if (node.Renderable != null) {
                this.RemoveRenderable(node.Renderable);
            }
            for (int i = 0; i < node.Children.Count; i++) {
                this._RemoveNodeRecursive(node.Children[i]);
            }
        }

        public void AddRenderable(Renderable renderable)
        {
            renderable.RenderListIndex = this._renderables.Count;
            this._renderables.Add(renderable);
        }

        public void RemoveRenderable(Renderable renderable)
        {
            this._renderables[this._renderables.Count - 1].RenderListIndex = renderable.RenderListIndex;
            this._renderables.RemoveAt(renderable.RenderListIndex);
            renderable.Free();
        }


        public void UpdateWorldMatrices()
        {
            if (this._transformDirtyList.Count > 0) {
                for (int i = 0; i < this._transformDirtyList.Count; i++) {
                    var dirtyNode = this._transformDirtyList[i];
                    if (! dirtyNode.TransformIsDirty) continue;
                    this._SearchTopDirty(dirtyNode).UpdateWorldMatrix();
                }
                this._transformDirtyList.Clear();
            }
        }

        private Node3D _SearchTopDirty(Node3D node)
        {
            Node3D topDirty = node;
            while (true) {
                if (node.TransformIsDirty) topDirty = node;
                if (node.Parent == null) return topDirty;
                node = node.Parent;
            }
        }
    }
}