using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.SceneGraph;

public abstract class Scene : Node3D
{
    public DirectionalLight MainLight { get; set; } = new DirectionalLight();

    public ColorF AmbientColor { get; set; } = new ColorF(.2f, .2f, .2f, 100f / 255f);


    public GizmosLayer Gizmos { get; } = new GizmosLayer();

    public Camera3D? Camera { get; set; } = null;

    private readonly List<IParticleSystem> _particleSystems = new List<IParticleSystem>();

    public IReadOnlyList<IParticleSystem> ParticleSystems => this._particleSystems;

    private readonly List<CanvasLayer> _canvasLayers = new List<CanvasLayer>();

    public IReadOnlyList<CanvasLayer> CanvasLayers => this._canvasLayers;

    private readonly List<Node3D> _transformDirtyList = new List<Node3D>();

    private readonly SwapPopList<Renderable> _renderables = new SwapPopList<Renderable>();

    private readonly Dictionary<Renderable, int> _renderableToIndex = new Dictionary<Renderable, int>();

    public IReadOnlyList<Renderable> Renderables => this._renderables;

    public Scene()
    {
        this.Scene = this;
    }

    public void AddParticleSystem(IParticleSystem particleSystem)
    {
        this._particleSystems.Add(particleSystem);
    }

    public void RemoveParticleSystem(IParticleSystem particleSystem)
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

    internal IEnumerable<Renderable> GetCulledRenderables(Camera3D camera)
    {
        throw new NotImplementedException();
    }

    public virtual void OnBeforeRender(Renderer renderer)
    {
        // 
    }

    public virtual void RenderImGui()
    {
        // 
    }

    public abstract void Update(float deltaTime);

    public void NotifyRenderableAdded(Renderable renderable)
    {
        if (this._renderableToIndex.ContainsKey(renderable))
        {
            return;
        }

        this._renderableToIndex.Add(renderable, this._renderables.Count);
        this._renderables.Add(renderable);
    }

    public void NotifyRenderableRemoved(Renderable renderable)
    {
        if (!this._renderableToIndex.TryGetValue(renderable, out var index))
        {
            return;
        }

        var lastRenderable = this._renderables[this._renderables.Count - 1];

        this._renderables.RemoveAt(index); // SwapPopList.RemoveAt() is O(1)
        this._renderableToIndex.Remove(renderable);

        if (lastRenderable != renderable)
        {
            this._renderableToIndex[lastRenderable] = index;
        }
    }

    public void EndUpdate()
    {
        for (int i = 0; i < this._canvasLayers.Count; i++)
        {
            this._canvasLayers[i].UpdateTransforms();
        }

        this.UpdateDirtyTransforms();
    }

    private void UpdateDirtyTransforms()
    {
        if (this._transformDirtyList.Count == 0) return;

        Matrix4x4 identity = Matrix4x4.Identity;

        for (int i = 0; i < this._transformDirtyList.Count; i++)
        {
            Node3D node = this._transformDirtyList[i];
            if (!node.TransformIsDirty) continue;

            // Search for the top dirty node
            Node3D topDirty = node;
            while (true)
            {
                if (node.TransformIsDirty) topDirty = node;
                if (node.Parent == null) break;
                node = node.Parent;
            }

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

    internal void NotifyNodeAdded(Node3D node)
    {
        this._transformDirtyList.Add(node);
    }

    internal void NotifyNodeRemoved(Node3D node)
    {
        this._transformDirtyList.Remove(node);
    }

    internal void NotifyTransformDirty(Node3D node)
    {
        this._transformDirtyList.Add(node);
    }

}