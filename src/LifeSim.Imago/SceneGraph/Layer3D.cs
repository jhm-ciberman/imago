using System;
using System.Collections.Generic;
using LifeSim.Imago.Assets.Textures;
using LifeSim.Imago.Rendering;
using LifeSim.Imago.SceneGraph.Cameras;
using LifeSim.Imago.SceneGraph.Immediate;
using LifeSim.Imago.SceneGraph.Nodes;
using LifeSim.Imago.SceneGraph.Picking;
using LifeSim.Support.Drawing;
using Veldrid;

namespace LifeSim.Imago.SceneGraph;

/// <summary>
/// A layer that renders 3D content. Contains a scene graph, camera, and environment.
/// </summary>
/// <remarks>
/// Layer3D absorbs the functionality of the old Stage class (render queues, dirty lists, etc.)
/// and can be used standalone for off-screen rendering or attached to Stage for main rendering.
/// </remarks>
public class Layer3D : ILayer, IDisposable
{
    /// <inheritdoc />
    public int ZOrder { get; init; } = 0;

    /// <inheritdoc />
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Gets or sets the clear color of the layer. If null, the layer will not be cleared
    /// and the previous frame will be visible.
    /// </summary>
    public Color? ClearColor { get; set; } = Color.Black;

    /// <summary>
    /// Gets or sets the root node of the 3D scene graph.
    /// </summary>
    public Node3D? Root
    {
        get => this._root;
        set
        {
            if (this._root == value) return;

            this._root?.DetachFromLayer();
            this._root = value;
            this._root?.AttachToLayer(this);
        }
    }

    private Node3D? _root;

    /// <summary>
    /// Gets or sets the camera used to render the layer.
    /// </summary>
    public Camera? Camera { get; set; } = null;

    /// <summary>
    /// Gets or sets the environment of the layer.
    /// </summary>
    public SceneEnvironment Environment { get; set; } = new SceneEnvironment();

    /// <summary>
    /// Gets the gizmos layer used to render debug and editor visuals.
    /// </summary>
    public GizmosLayer Gizmos { get; } = new GizmosLayer();

    /// <summary>
    /// Gets the picking manager for object selection in this layer.
    /// </summary>
    public PickingManager Picking { get; } = PickingManager.Instance;

    private readonly List<Node3D> _transformDirtyList = new();
    private readonly List<Renderable> _renderables = new();
    private readonly List<Renderable> _dirtyRenderables = new();
    private readonly List<ImmediateRenderable3D> _immediateRenderables = new();
    private readonly HashSet<Skeleton> _skeletons = new();
    private readonly List<IParticleSystem> _particleSystems = new();

    internal RenderQueue PickingRenderQueue { get; } = new RenderQueue(RenderQueues.Picking);
    internal RenderQueue OpaqueRenderQueue { get; } = new RenderQueue(RenderQueues.Opaque);
    internal RenderQueue TransparentRenderQueue { get; } = new RenderQueue(RenderQueues.Transparent);
    internal RenderQueue[] ShadowCasterRenderQueues { get; } = new RenderQueue[4];

    private bool _invalidateRendererPipelines = false;

    /// <summary>
    /// Gets the list of particle systems in the layer.
    /// </summary>
    public IReadOnlyList<IParticleSystem> ParticleSystems => this._particleSystems;

    /// <summary>
    /// Adds a particle system to the layer.
    /// </summary>
    /// <param name="particleSystem">The particle system to add.</param>
    public void AddParticleSystem(IParticleSystem particleSystem)
    {
        this._particleSystems.Add(particleSystem);
    }

    /// <summary>
    /// Removes a particle system from the layer.
    /// </summary>
    /// <param name="particleSystem">The particle system to remove.</param>
    public void RemoveParticleSystem(IParticleSystem particleSystem)
    {
        this._particleSystems.Remove(particleSystem);
    }

    /// <summary>
    /// Gets the list of immediate-mode renderables.
    /// </summary>
    public IReadOnlyList<ImmediateRenderable3D> ImmediateRenderables => this._immediateRenderables;

    /// <summary>
    /// Adds an immediate-mode renderable to the layer.
    /// </summary>
    /// <param name="immediateRenderable">The immediate renderable to add.</param>
    public void AddImmediateRenderable(ImmediateRenderable3D immediateRenderable)
    {
        this._immediateRenderables.Add(immediateRenderable);
    }

    /// <summary>
    /// Removes an immediate-mode renderable from the layer.
    /// </summary>
    /// <param name="immediateRenderable">The immediate renderable to remove.</param>
    public void RemoveImmediateRenderable(ImmediateRenderable3D immediateRenderable)
    {
        this._immediateRenderables.Remove(immediateRenderable);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Layer3D"/> class.
    /// </summary>
    public Layer3D()
    {
        for (int i = 0; i < this.ShadowCasterRenderQueues.Length; i++)
        {
            this.ShadowCasterRenderQueues[i] = new RenderQueue(RenderQueues.ShadowCaster);
        }
    }

    private TextureSampleCount _multiSampleCount = TextureSampleCount.Count1;

    /// <summary>
    /// Gets or sets the multi-sample anti-aliasing (MSAA) count for rendering.
    /// </summary>
    public TextureSampleCount MultiSampleCount
    {
        get => this._multiSampleCount;
        set => this.SetProperty(ref this._multiSampleCount, value);
    }

    private bool _forceWireframe = false;

    /// <summary>
    /// Gets or sets a value indicating whether to force wireframe rendering for all objects.
    /// </summary>
    public bool ForceWireframe
    {
        get => this._forceWireframe;
        set => this.SetProperty(ref this._forceWireframe, value);
    }

    private bool _enableFog = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable fog.
    /// </summary>
    public bool EnableFog
    {
        get => this._enableFog;
        set => this.SetProperty(ref this._enableFog, value);
    }

    private bool _enablePixelPerfectShadows = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable pixel-perfect shadows.
    /// </summary>
    public bool EnablePixelPerfectShadows
    {
        get => this._enablePixelPerfectShadows;
        set => this.SetProperty(ref this._enablePixelPerfectShadows, value);
    }

    private bool _lightingHalfLambert = true;

    /// <summary>
    /// Gets or sets a value indicating whether to use Half-Lambert lighting model.
    /// </summary>
    public bool LightingHalfLambert
    {
        get => this._lightingHalfLambert;
        set => this.SetProperty(ref this._lightingHalfLambert, value);
    }

    private int _cascadesCount = 4;

    /// <summary>
    /// Gets or sets the number of cascades for shadow mapping.
    /// </summary>
    public int CascadesCount
    {
        get => this._cascadesCount;
        set => this.SetProperty(ref this._cascadesCount, value);
    }

    private bool SetProperty<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;

        field = value;
        this._invalidateRendererPipelines = true;
        return true;
    }

    /// <inheritdoc />
    public void Update(float deltaTime)
    {
        this.Gizmos.Update(deltaTime);
        // Note: Picking.Update is called by Stage.Update with the correct isCursorOverUi value
    }

    /// <summary>
    /// Prepares the layer for rendering by updating transforms, skeletons, and render queues.
    /// </summary>
    /// <param name="renderTexture">The render texture that will be used for rendering.</param>
    public void PrepareForRender(RenderTexture renderTexture)
    {
        if (this.MultiSampleCount != renderTexture.SampleCount)
        {
            this.MultiSampleCount = renderTexture.SampleCount;
        }

        var camera = this.Camera;
        if (camera == null) return;

        var shadowMap = this.Environment.MainLight.ShadowMap;
        shadowMap.UpdateSplitDistances(camera, out int cascadesCount);
        this.CascadesCount = cascadesCount;

        if (this._invalidateRendererPipelines)
        {
            this._invalidateRendererPipelines = false;

            for (int i = 0; i < this._renderables.Count; i++)
            {
                this._renderables[i].InvalidatePipeline();
            }
        }

        if (this._transformDirtyList.Count > 0)
        {
            for (int i = 0; i < this._transformDirtyList.Count; i++)
            {
                Node3D node = this._transformDirtyList[i];
                if (!node.LocalTransformIsDirty) continue;

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

        if (this._dirtyRenderables.Count > 0)
        {
            foreach (var renderable in this._dirtyRenderables)
            {
                renderable.Update(this);
            }
            this._dirtyRenderables.Clear();
        }

        foreach (var skeleton in this._skeletons)
        {
            skeleton.Update();
        }
    }

    /// <summary>
    /// Notifies the layer that the transform of the specified node is not dirty.
    /// </summary>
    /// <param name="node">The node.</param>
    internal void NotifyTransformNotDirty(Node3D node)
    {
        this._transformDirtyList.Remove(node);
    }

    /// <summary>
    /// Notifies the layer that the transform of the specified node is dirty.
    /// </summary>
    /// <param name="node">The node.</param>
    internal void NotifyTransformDirty(Node3D node)
    {
        this._transformDirtyList.Add(node);
    }

    /// <summary>
    /// Notifies the layer that the renderable pipeline of the specified renderable is dirty.
    /// </summary>
    /// <param name="renderable">The renderable.</param>
    internal void NotifyRenderablePipelineDirty(Renderable renderable)
    {
        this._dirtyRenderables.Add(renderable);
    }

    /// <summary>
    /// Notifies the layer that the renderable queue render flags of the specified renderable have changed.
    /// </summary>
    /// <param name="renderable">The renderable.</param>
    /// <param name="oldQueuesFlags">The old render flags.</param>
    /// <param name="newQueuesFlags">The new render flags.</param>
    internal void NotifyRenderableRenderQueueChanged(Renderable renderable, RenderQueues oldQueuesFlags, RenderQueues newQueuesFlags)
    {
        this.OpaqueRenderQueue.UpdateRenderableRenderFlags(renderable, oldQueuesFlags, newQueuesFlags);
        this.TransparentRenderQueue.UpdateRenderableRenderFlags(renderable, oldQueuesFlags, newQueuesFlags);
        this.PickingRenderQueue.UpdateRenderableRenderFlags(renderable, oldQueuesFlags, newQueuesFlags);

        var queues = this.ShadowCasterRenderQueues;
        for (int i = 0; i < queues.Length; i++)
        {
            queues[i].UpdateRenderableRenderFlags(renderable, oldQueuesFlags, newQueuesFlags);
        }
    }

    /// <summary>
    /// Notifies the layer that the renderable skeleton of the specified renderable has changed.
    /// </summary>
    /// <param name="renderable">The renderable.</param>
    /// <param name="oldSkeleton">The old skeleton.</param>
    /// <param name="newSkeleton">The new skeleton.</param>
    internal void NotifyRenderableSkeletonChanged(Renderable renderable, Skeleton? oldSkeleton, Skeleton? newSkeleton)
    {
        if (oldSkeleton != null)
        {
            this._skeletons.Remove(oldSkeleton);
        }

        if (newSkeleton != null)
        {
            this._skeletons.Add(newSkeleton);
        }
    }

    internal void AddRenderable(Renderable renderable)
    {
        this._renderables.Add(renderable);
        this.NotifyRenderablePipelineDirty(renderable);
        this.NotifyRenderableRenderQueueChanged(renderable, RenderQueues.None, renderable.RenderQueues);
        this.NotifyRenderableSkeletonChanged(renderable, null, renderable.Skeleton);
    }

    internal void RemoveRenderable(Renderable renderable)
    {
        this._renderables.Remove(renderable);
        this.NotifyRenderablePipelineDirty(renderable);
        this.NotifyRenderableRenderQueueChanged(renderable, renderable.RenderQueues, RenderQueues.None);
    }

    /// <summary>
    /// Disposes the layer and releases associated resources.
    /// </summary>
    public void Dispose()
    {
        this.Root?.Dispose();
    }
}
