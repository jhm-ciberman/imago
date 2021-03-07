using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;
using Veldrid;

namespace LifeSim.Engine.SceneGraph
{
    public class Scene3D : ILayer
    {
        public Camera3D? activeCamera = null;

        public DirectionalLight mainLight = new DirectionalLight();
        
        public Vector3 ambientColor = new Vector3(.2f, .2f, .2f);

        public RgbaFloat clearColor = new RgbaFloat(0.84f, 0.84f, 0.86f, 1.0f);
        //private RgbaFloat _clearColor = new RgbaFloat(0.04f, 0.04f, 0.06f, 1.0f);

        private Node3D _root = new Node3D();
        public Node3D root => this._root;

        private List<Renderable3D> _renderables = new List<Renderable3D>();
        public IReadOnlyList<Renderable3D> renderables => this._renderables;

        public Scene3D()
        {
            this._root.onChildAdded += this._AddRenderables;
            this._root.onChildRemoved += this._RemoveRenderables;
        }

        public void Add(Node3D node)
        {
            this._root.Add(node);
        }

        public void Remove(Node3D node)
        {
            this._root.Remove(node);
        }

        private void _AddRenderables(Node3D node)
        {
            if (node is Renderable3D renderable) {
                this._renderables.Add(renderable);
            }
            foreach (var child in node.children) {
                this._AddRenderables(child);
            }
        }

        private void _RemoveRenderables(Node3D node)
        {
            if (node is Renderable3D renderable) {
                this._renderables.Remove(renderable);
            }
            foreach (var child in node.children) {
                this._RemoveRenderables(child);
            }
        }

        public void UpdateWorldMatrices()
        {
            this._root?.UpdateWorldMatrix();
        }

        void ILayer.Render(GPURenderer renderer)
        {
            renderer.RenderScene3D(this);
        }
    }
}