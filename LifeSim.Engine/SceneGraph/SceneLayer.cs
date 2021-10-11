using System.Collections.Generic;
using System.Numerics;
using LifeSim.Core;
using LifeSim.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class SceneLayer
    {
        public DirectionalLight MainLight { get; set; } = new DirectionalLight();

        public ColorF AmbientColor { get; set; } = new ColorF(.2f, .2f, .2f);

        public ColorF ClearColor { get; set; } = new ColorF(0.84f, 0.84f, 0.86f, 1.0f);

        private readonly SwapPopList<Renderable> _renderables = new SwapPopList<Renderable>();

        public IReadOnlyList<Renderable> Renderables => this._renderables;

        private readonly List<Node3D> _transformDirtyList = new List<Node3D>();

        public GizmosLayer Gizmos { get; } = new GizmosLayer();

        private readonly List<ParticleSystem> _particleSystems = new List<ParticleSystem>();

        private readonly Node3D _root;

        protected class RootNode : Node3D
        {
            public RootNode(SceneLayer scene) : base()
            {
                this.Scene = scene;
            }
        }

        public void Render(Renderer renderer, Camera3D camera3D)
        {
            if (this.Renderables.Count > 0)
            {
                renderer.SceneRenderer.Render(this.Renderables, this.MainLight, this.AmbientColor, this.ClearColor, camera3D);
            }

            if (this.Gizmos.Lines.Count > 0)
            {
                renderer.GizmosRenderer.Render(this.Gizmos.Lines, camera3D);
            }

            for (int i = 0; i < this._particleSystems.Count; i++)
            {
                this._particleSystems[i].Render(renderer, camera3D);
            }
        }

        public SceneLayer()
        {
            this._root = new RootNode(this);
        }

        public void AddParticleSystem(ParticleSystem particleSystem)
        {
            this._particleSystems.Add(particleSystem);
        }

        public void RemoveParticleSystem(ParticleSystem particleSystem)
        {
            this._particleSystems.Remove(particleSystem);
        }

        public void AddNode(Node3D node)
        {
            this._root.Add(node);
            this._transformDirtyList.Add(node);
            this._SubscribeRecursively(node);
        }

        public void RemoveNode(Node3D node)
        {
            this._root.Remove(node);
            if (node.TransformIsDirty)
            {
                this._transformDirtyList.Remove(node);
            }
            this._UnsubscribeRecursively(node);
        }

        private void _SubscribeRecursively(Node3D node)
        {
            node.OnTransformDirty += this._OnTransformDirtyEvent;
            node.OnNodeAdded += this._OnNodeAddedEvent;
            node.OnNodeRemoved += this._OnNodeRemovedEvent;

            if (node is RenderNode3D renderNode)
            {
                if (renderNode.Renderable != null)
                {
                    this.AddRenderable(renderNode.Renderable);
                }
                renderNode.OnRenderableAdded += this._OnRenderableAddedEvent;
                renderNode.OnRenderableRemoved += this._OnRenderableRemovedEvent;
            }

            for (int i = 0; i < node.Children.Count; i++)
            {
                this._SubscribeRecursively(node.Children[i]);
            }
        }

        private void _UnsubscribeRecursively(Node3D node)
        {
            node.OnTransformDirty -= this._OnTransformDirtyEvent;
            node.OnNodeAdded -= this._OnNodeAddedEvent;
            node.OnNodeRemoved -= this._OnNodeRemovedEvent;

            if (node is RenderNode3D renderNode)
            {
                if (renderNode.Renderable != null)
                {
                    this.RemoveRenderable(renderNode.Renderable);
                }
                renderNode.OnRenderableAdded -= this._OnRenderableAddedEvent;
                renderNode.OnRenderableRemoved -= this._OnRenderableRemovedEvent;
            }

            for (int i = 0; i < node.Children.Count; i++)
            {
                this._UnsubscribeRecursively(node.Children[i]);
            }
        }

        private void _OnNodeRemovedEvent(Node3D sender, Node3D node)
        {
            this._UnsubscribeRecursively(node);
        }

        private void _OnNodeAddedEvent(Node3D sender, Node3D node)
        {
            this._SubscribeRecursively(node);
        }

        private void _OnRenderableRemovedEvent(Node3D sender, Renderable renderable)
        {
            this.RemoveRenderable(renderable);
        }

        private void _OnRenderableAddedEvent(Node3D sender, Renderable renderable)
        {
            this.AddRenderable(renderable);
        }

        internal void _OnTransformDirtyEvent(Node3D node)
        {
            if (node.Scene == this)
            {
                this._transformDirtyList.Add(node);
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


        public void Update()
        {
            if (this._transformDirtyList.Count > 0)
            {
                Matrix4x4 identity = Matrix4x4.Identity;

                for (int i = 0; i < this._transformDirtyList.Count; i++)
                {
                    var dirtyNode = this._transformDirtyList[i];
                    if (!dirtyNode.TransformIsDirty) continue;
                    this._SearchTopDirty(dirtyNode).UpdateWorldMatrix(ref identity);
                }
                this._transformDirtyList.Clear();
            }
        }

        private Node3D _SearchTopDirty(Node3D node)
        {
            Node3D topDirty = node;
            while (true)
            {
                if (node.TransformIsDirty) topDirty = node;
                if (node.Parent == null) return topDirty;
                node = node.Parent;
            }
        }
    }
}