using System.Collections.Generic;
using LifeSim.Core;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class Scene3D
    {
        public DirectionalLight mainLight = new DirectionalLight();
        
        public ColorF ambientColor = new ColorF(.2f, .2f, .2f);

        public ColorF clearColor = new ColorF(0.84f, 0.84f, 0.86f, 1.0f);
        //private RgbaFloat _clearColor = new RgbaFloat(0.04f, 0.04f, 0.06f, 1.0f);

        private readonly SwapPopList<Node3D> _children = new SwapPopList<Node3D>();
        public IReadOnlyList<Node3D> children => this._children;

        private SwapPopList<Renderable> _renderables = new SwapPopList<Renderable>();
        public IReadOnlyList<Renderable> renderables => this._renderables;

        private List<RenderNode3D> _instanceDataDirtyList = new List<RenderNode3D>();
        private List<Node3D> _transformDirtyList = new List<Node3D>();

        internal SceneStorage storage { get; private set; }

        public Scene3D(SceneStorage sceneStorage)
        {
            this.storage = sceneStorage;
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
            this._transformDirtyList.Add(node);
        }

        internal void _OnInstanceDataDirty(RenderNode3D node)
        {
            this._instanceDataDirtyList.Add(node);
        }

        private void _AddNodeToRecursive(Node3D node)
        {
            node._scene = this;
            if (node is RenderNode3D renderable) {
                this._OnInstanceDataDirty(renderable);
                this._AddToRenderList(renderable);
            }
            for (int i = 0; i < node.children.Count; i++) {
                this._AddNodeToRecursive(node.children[i]);
            }
        }

        private void _RemoveNodeRecursive(Node3D node)
        {
            node._scene = null;
            if (node is RenderNode3D renderable) {
                this._RemoveFromRenderList(renderable);
            }
            for (int i = 0; i < node.children.Count; i++) {
                this._RemoveNodeRecursive(node.children[i]);
            }
        }

        private void _AddToRenderList(RenderNode3D node)
        {
            if (node._renderable != null) return;
            if (node.mesh == null) return;
            if (node.material == null) return;

            var renderable = new Renderable(this.storage, node.mesh, node.material);
            this.AddRenderable(renderable);
            node._renderable = renderable;
        }

        private void _RemoveFromRenderList(RenderNode3D node)
        {
            if (node._renderable == null) return;
            this.RemoveRenderable(node._renderable);
            node._renderable = null;
        }

        public void AddRenderable(Renderable renderable)
        {
            renderable.renderListIndex = this._renderables.Count;
            this._renderables.Add(renderable);
        }

        public void RemoveRenderable(Renderable renderable)
        {
            this._renderables[this._renderables.Count - 1].renderListIndex = renderable.renderListIndex;
            this._renderables.RemoveAt(renderable.renderListIndex);
            renderable.Free();
        }


        public void UpdateWorldMatrices()
        {
            if (this._transformDirtyList.Count > 0) {
                for (int i = 0; i < this._transformDirtyList.Count; i++) {
                    var dirtyNode = this._transformDirtyList[i];
                    if (! dirtyNode.transformIsDirty) continue;
                    this._SearchTopDirty(dirtyNode).UpdateWorldMatrix();
                }
                this._transformDirtyList.Clear();
            }
        }

        public void UpdateInstanceData()
        {
            for (int i = 0; i < this._instanceDataDirtyList.Count; i++) {
                var node = this._instanceDataDirtyList[i];
                node.UpdateInstanceData();
            }
            this._instanceDataDirtyList.Clear();
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