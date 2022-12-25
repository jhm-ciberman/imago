using System;
using System.Collections.Generic;
using LifeSim.Engine.Controls;
using LifeSim.Engine.Rendering;
using LifeSim.Support;

namespace LifeSim.Engine.SceneGraph;

public class Scene
{
    private class RootNode : Node3D
    {
        public RootNode(Scene scene)
        {
            this.Scene = scene;
        }
    }

    /// <summary>
    /// Gets or sets the background color of the scene.
    /// </summary>
    public Color? BackgroundColor { get; set; } = Color.CoolGray;

    /// <summary>
    /// Gets or sets the main light of the scene.
    /// </summary>
    public DirectionalLight MainLight { get; set; } = new DirectionalLight();

    /// <summary>
    /// Gets or sets the ambient color of the scene.
    /// </summary>
    public ColorF AmbientColor { get; set; } = new ColorF(.2f, .2f, .2f, 100f / 255f);

    /// <summary>
    /// Gets the gizmos layer used to render gizmos.
    /// </summary>
    public GizmosLayer Gizmos { get; } = new GizmosLayer();

    /// <summary>
    /// Gets or sets the camera used to render the scene.
    /// </summary>
    public Camera3D? Camera { get; set; } = null;

    /// <summary>
    /// Gets the canvas layers used to render UI elements.
    /// </summary>
    public IReadOnlyList<CanvasLayer> CanvasLayers => this._canvasLayers;

    /// <summary>
    /// Gets or sets the UI page to render UI elements.
    /// </summary>
    public UIPage? UIPage { get; set; } = null;

    /// <summary>
    /// Gets the particle systems used to render particles.
    /// </summary>
    public IReadOnlyList<IParticleSystem> ParticleSystems => this._particleSystems;

    /// <summary>
    /// Gets or sets the fog color.
    /// </summary>
    public ColorF FogColor { get; set; } = new ColorF("#6d6b4e");

    /// <summary>
    /// Gets or sets the start distance of the fog.
    /// </summary>
    public float FogStart { get; set; } = 50f;

    /// <summary>
    /// Gets or sets the end distance of the fog.
    /// </summary>
    public float FogEnd { get; set; } = 300f;

    /// <summary>
    /// Gets the picking manager used to pick objects.
    /// </summary>
    public PickingManger Picking { get; } = new PickingManger();

    /// <summary>
    /// Gets the root node of the scene.
    /// </summary>
    public Node3D Root { get; }

    private readonly List<IParticleSystem> _particleSystems = new List<IParticleSystem>();

    private readonly List<CanvasLayer> _canvasLayers = new List<CanvasLayer>();

    private readonly List<Node3D> _transformDirtyList = new List<Node3D>();

    private readonly List<IImmediateRenderable> _immediateRenderables = new List<IImmediateRenderable>();

    /// <summary>
    /// Gets the immediate renderables used to render objects immediately.
    /// </summary>
    public IReadOnlyList<IImmediateRenderable> ImmediateRenderables => this._immediateRenderables;

    /// <summary>
    /// Initializes a new instance of the <see cref="Scene"/> class.
    /// </summary>
    public Scene()
    {
        this.Root = new RootNode(this);
    }

    /// <summary>
    /// Adds a particle system to the scene.
    /// </summary>
    /// <param name="particleSystem">The particle system to add.</param>
    public void AddParticleSystem(IParticleSystem particleSystem)
    {
        this._particleSystems.Add(particleSystem);
    }

    /// <summary>
    /// Removes a particle system from the scene.
    /// </summary>
    /// <param name="particleSystem">The particle system to remove.</param>
    public void RemoveParticleSystem(IParticleSystem particleSystem)
    {
        this._particleSystems.Remove(particleSystem);
    }

    /// <summary>
    /// Adds a canvas layer to the scene.
    /// </summary>
    /// <param name="canvasLayer">The canvas layer to add.</param>
    public void AddCanvasLayer(CanvasLayer canvasLayer)
    {
        this._canvasLayers.Add(canvasLayer);
    }

    /// <summary>
    /// Removes a canvas layer from the scene.
    /// </summary>
    /// <param name="canvasLayer">The canvas layer to remove.</param>
    public void RemoveCanvasLayer(CanvasLayer canvasLayer)
    {
        this._canvasLayers.Remove(canvasLayer);
    }

    /// <summary>
    /// Adds an immediate renderable to the scene.
    /// </summary>
    /// <param name="immediateRenderable">The immediate renderable to add.</param>
    public void AddImmediateRenderable(IImmediateRenderable immediateRenderable)
    {
        this._immediateRenderables.Add(immediateRenderable);
    }

    /// <summary>
    /// Removes an immediate renderable from the scene.
    /// </summary>
    /// <param name="immediateRenderable">The immediate renderable to remove.</param>
    public void RemoveImmediateRenderable(IImmediateRenderable immediateRenderable)
    {
        this._immediateRenderables.Remove(immediateRenderable);
    }

    /// <summary>
    /// Called before the scene is rendered.
    /// </summary>
    public virtual void OnBeforeRender()
    {
        // 
    }

    /// <summary>
    /// Called when the ImGui UI is rendered. You can use this to render your own ImGui UI.
    /// </summary>
    public virtual void RenderImGui()
    {
        // 
    }

    /// <summary>
    /// Updates the scene.
    /// </summary>
    /// <param name="deltaTime">The time since the last update in seconds.</param>
    public virtual void Update(float deltaTime)
    {
        this.UIPage?.Update(deltaTime);
    }

    /// <summary>
    /// Updates the transforms of the scene.
    /// </summary>
    public void UpdateTransforms()
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

    /// <summary>
    /// Notifies the scene that the transform of the specified node is not dirty.
    /// </summary>
    /// <param name="node">The node.</param>
    internal void NotifyTransformNotDirty(Node3D node)
    {
        this._transformDirtyList.Remove(node);
    }

    /// <summary>
    /// Notifies the scene that the transform of the specified node is dirty.
    /// </summary>
    /// <param name="node">The node.</param>
    internal void NotifyTransformDirty(Node3D node)
    {
        this._transformDirtyList.Add(node);
    }
}