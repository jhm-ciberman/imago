using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace LifeSim.Engine.SceneGraph
{
    public class Scene3D
    {
        public DirectionalLight mainLight = new DirectionalLight();
        
        public Vector3 ambientColor = new Vector3(.2f, .2f, .2f);

        public RgbaFloat clearColor = new RgbaFloat(0.84f, 0.84f, 0.86f, 1.0f);
        //private RgbaFloat _clearColor = new RgbaFloat(0.04f, 0.04f, 0.06f, 1.0f);

        private Node3D _root = new Node3D();
        public Node3D root => this._root;

        private List<RenderNode3D> _renderables = new List<RenderNode3D>();
        public IReadOnlyList<RenderNode3D> renderables => this._renderables;

        private List<Node3D> _dirtyList = new List<Node3D>();

        public Scene3D()
        {
            this._root.onEvent += this._OnEvent;
        }

        public void Add(Node3D node)
        {
            this._root.Add(node);
        }

        public void Remove(Node3D node)
        {
            this._root.Remove(node);
        }

        private void _OnEvent(Event<Node3D> e)
        {
            switch (e.type)
            {
                case EventType.ChildAdded:
                    this._AddRenderables(e.node);
                    this._AddToDirtyList(e.node);
                    break;
                case EventType.ChildRemoved:
                    this._RemoveRenderables(e.node);
                    this._AddToDirtyList(e.node);
                    break;
                case EventType.TransformDirty:
                    this._AddToDirtyList(e.node);
                    break;   
            }   
        }

        private void _AddRenderables(Node3D node)
        {
            if (node is RenderNode3D renderable) {
                this._renderables.Add(renderable);
            }
            foreach (var child in node.children) {
                this._AddRenderables(child);
            }
        }

        private void _RemoveRenderables(Node3D node)
        {
            if (node is RenderNode3D renderable) {
                this._renderables.Remove(renderable);
            }
            foreach (var child in node.children) {
                this._RemoveRenderables(child);
            }
        }

        private void _AddToDirtyList(Node3D node)
        {
            this._dirtyList.Add(node);
        }

        public void UpdateWorldMatrices()
        {
            if (this._dirtyList.Count > 0) {
                foreach (var dirtyNode in this._dirtyList) {
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