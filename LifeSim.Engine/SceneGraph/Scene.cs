using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Controls;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.SceneGraph;

public class Scene : Node3D
{
    public Color? BackgroundColor { get; set; } = Color.CoolGray;
    public DirectionalLight MainLight { get; set; } = new DirectionalLight();
    public ColorF AmbientColor { get; set; } = new ColorF(.2f, .2f, .2f, 100f / 255f);
    public GizmosLayer Gizmos { get; } = new GizmosLayer();
    public Camera3D? Camera { get; set; } = null;
    public IReadOnlyList<CanvasLayer> CanvasLayers => this._canvasLayers;

    public UILayer? UILayer { get; set; } = null;

    public IReadOnlyList<IParticleSystem> ParticleSystems => this._particleSystems;

    public ColorF FogColor { get; set; } = new ColorF("#6d6b4e");
    public float FogStart { get; set; } = 50f;
    public float FogEnd { get; set; } = 300f;

    private readonly List<IParticleSystem> _particleSystems = new List<IParticleSystem>();
    private readonly List<CanvasLayer> _canvasLayers = new List<CanvasLayer>();

    private readonly List<Node3D> _transformDirtyList = new List<Node3D>();

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

    public virtual void OnBeforeRender()
    {
        // 
    }

    public virtual void RenderImGui()
    {
        // 
    }

    public virtual void Update(float deltaTime)
    {
        //

        this.UILayer?.Update(deltaTime);
    }

    public void UpdateSceneDirtyTransforms()
    {
        for (int i = 0; i < this._canvasLayers.Count; i++)
        {
            this._canvasLayers[i].UpdateTransforms();
        }

        if (this._transformDirtyList.Count == 0) return;

        for (int i = 0; i < this._transformDirtyList.Count; i++)
        {
            Node3D node = this._transformDirtyList[i];
            if (!node.LocalTransformIsDirty) continue;

            // Search for the top dirty node
            Node3D topDirty = node;
            while (true)
            {
                if (node.LocalTransformIsDirty) topDirty = node;
                if (node.Parent == null) break;
                node = node.Parent;
            }

            topDirty.UpdateTransform();
        }

        this._transformDirtyList.Clear();
    }

    internal void NotifyTransformNotDirty(Node3D node)
    {
        this._transformDirtyList.Remove(node);
    }

    internal void NotifyTransformDirty(Node3D node)
    {
        this._transformDirtyList.Add(node);
    }
}