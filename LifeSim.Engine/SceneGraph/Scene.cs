using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.SceneGraph
{
    public abstract class Scene
    {
        public App App { get; }

        public DirectionalLight MainLight { get; set; } = new DirectionalLight();

        public ColorF AmbientColor { get; set; } = new ColorF(.2f, .2f, .2f);

        public ColorF ClearColor { get; set; } = new ColorF(0.84f, 0.84f, 0.86f, 1.0f);


        public GizmosLayer Gizmos { get; } = new GizmosLayer();

        public ICamera? Camera { get; set; } = null;

        private readonly List<ParticleSystem> _particleSystems = new List<ParticleSystem>();

        public IReadOnlyList<ParticleSystem> ParticleSystems => this._particleSystems;


        private readonly SwapPopList<Renderable> _renderables = new SwapPopList<Renderable>();

        private readonly List<CanvasLayer> _canvasLayers = new List<CanvasLayer>();

        public IReadOnlyList<CanvasLayer> CanvasLayers => this._canvasLayers;

        public IReadOnlyList<Renderable> Renderables => this._renderables;

        private readonly List<Node3D> _transformDirtyList = new List<Node3D>();

        private RenderQueue _shadowmapReadOnlyQueue = new RenderQueue();

        private RenderQueue _forwardReadOnlyQueue = new RenderQueue();

        private RenderQueue _shadowmapReadWriteQueue = new RenderQueue();

        private RenderQueue _forwardReadWriteQueue = new RenderQueue();

        public IReadOnlyList<Renderable> ForwardQueue => this._forwardReadWriteQueue;

        public IReadOnlyList<Renderable> ShadowmapQueue => this._shadowmapReadWriteQueue;

        private readonly Node3D _root = new Node3D();

        public Scene(App app)
        {
            this.App = app;
            this._root.OnNodeAdded += this._OnNodeAddedEvent;
            this._root.OnNodeRemoved += this._OnNodeRemovedEvent;
        }

        public void Add(Node3D node)
        {
            this._root.Add(node);
        }

        public void Remove(Node3D node)
        {
            this._root.Remove(node);
        }

        public void AddParticleSystem(ParticleSystem particleSystem)
        {
            this._particleSystems.Add(particleSystem);
        }

        public void RemoveParticleSystem(ParticleSystem particleSystem)
        {
            this._particleSystems.Remove(particleSystem);
        }

        public void AddCanvasLayer(CanvasLayer canvasLayer)
        {
            this._canvasLayers.Add(canvasLayer);
        }

        public void RemoveCanvasLayer(CanvasLayer canvasLayer)
        {
            this._canvasLayers.Remove(canvasLayer);
        }


        public virtual void RenderFrame(Renderer renderer)
        {
            // 
        }

        public virtual void RenderImGui()
        {
            // 
        }

        public abstract void Update(float deltaTime);


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

        private void _OnNodeRemovedEvent(Node3D node)
        {
            this._transformDirtyList.Remove(node);
            this._UnsubscribeRecursively(node);
        }

        private void _OnNodeAddedEvent(Node3D node)
        {
            this._transformDirtyList.Add(node);
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
            this._transformDirtyList.Add(node);
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

        public void EndUpdate()
        {
            this._UpdateTransforms();
            this._UpdateRenderQueues();
        }

        private void _UpdateTransforms()
        {
            for (int i = 0; i < this._canvasLayers.Count; i++)
            {
                this._canvasLayers[i].UpdateTransforms();
            }

            if (this._transformDirtyList.Count == 0) return;

            Matrix4x4 identity = Matrix4x4.Identity;

            for (int i = 0; i < this._transformDirtyList.Count; i++)
            {
                var dirtyNode = this._transformDirtyList[i];
                if (!dirtyNode.TransformIsDirty) continue;
                Node3D topDirty = this._SearchTopDirty(dirtyNode);
                if (topDirty.Parent != null)
                {
                    topDirty.UpdateWorldMatrix(ref topDirty.Parent.WorldMatrix);
                }
                else
                {
                    topDirty.UpdateWorldMatrix(ref identity);
                }
            }
            this._transformDirtyList.Clear();
        }

        public void BeginUpdate()
        {
            // Swap queues for next frame
            (this._shadowmapReadOnlyQueue, this._shadowmapReadWriteQueue) = (this._shadowmapReadWriteQueue, this._shadowmapReadOnlyQueue);
            (this._forwardReadOnlyQueue, this._forwardReadWriteQueue) = (this._forwardReadWriteQueue, this._forwardReadOnlyQueue);
        }

        private void _UpdateRenderQueues()
        {
            var camera = this.Camera;
            if (camera == null) return;

            var matrix = this.MainLight.GetShadowMapMatrix(camera.Position);
            var frustum = new Veldrid.Utilities.BoundingFrustum(matrix);
            this._shadowmapReadWriteQueue.CameraPosition = camera.Position;
            this._shadowmapReadWriteQueue.AddToRenderQueue(this.Renderables, ref frustum);

            var cameraFrustum = camera.FrustumForCulling;
            this._forwardReadWriteQueue.CameraPosition = camera.Position;
            this._forwardReadWriteQueue.ViewProjectionMatrix = camera.ViewProjectionMatrix;
            this._forwardReadWriteQueue.AddToRenderQueue(this.Renderables, ref cameraFrustum);

            this._shadowmapReadWriteQueue.Sort();
            this._forwardReadWriteQueue.Sort();
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