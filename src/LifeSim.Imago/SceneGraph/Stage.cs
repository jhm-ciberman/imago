using System;
using System.Collections.Generic;
using LifeSim.Imago.Rendering;
using LifeSim.Imago.SceneGraph.Nodes;
using LifeSim.Imago.SceneGraph.Picking;
using LifeSim.Imago.Textures;

namespace LifeSim.Imago.SceneGraph;

public class Stage
{
    /// <summary>
    /// Occurs when the current <see cref="Scene"/> changes.
    /// </summary>
    public event EventHandler<SceneChangedEventArgs>? SceneChanged;

    private class EmptyScene : Scene { }

    private static readonly Scene _emptyScene = new EmptyScene()
    {
        Name = "Empty Scene",
        DisposeOnDetach = false,
    };

    /// <summary>
    /// Gets the gizmos layer used to render gizmos.
    /// </summary>
    public GizmosLayer Gizmos { get; } = new GizmosLayer();

    /// <summary>
    /// Gets the picking manager used to pick objects.
    /// </summary>
    public PickingManager Picking { get; } = new PickingManager();

    /// <summary>
    /// Gets the current scene.
    /// </summary>
    public Scene Scene { get; private set; } = _emptyScene;

    private readonly List<Node3D> _transformDirtyList = new List<Node3D>();

    internal RenderQueue PickingRenderQueue { get; } = new RenderQueue(RenderQueues.Picking);

    internal RenderQueue OpaqueRenderQueue { get; } = new RenderQueue(RenderQueues.Opaque);

    internal RenderQueue TransparentRenderQueue { get; } = new RenderQueue(RenderQueues.Transparent);

    internal RenderQueue[] ShadowCasterRenderQueues { get; } = new RenderQueue[4];

    private readonly List<Renderable> _renderables = new();
    private readonly List<Renderable> _dirtyRenderables = new();
    private readonly List<ImmediateRenderable3D> _immediateRenderables = new();
    private readonly HashSet<Skeleton> _skeletons = new();

    private bool _invalidateRendererPipelines = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Stage"/> class.
    /// </summary>
    public Stage()
    {
        for (int i = 0; i < this.ShadowCasterRenderQueues.Length; i++)
        {
            this.ShadowCasterRenderQueues[i] = new RenderQueue(RenderQueues.ShadowCaster);
        }
    }

    /// <summary>
    /// Changes the current scene.
    /// </summary>
    /// <param name="scene">The new scene.</param>
    public void ChangeScene(Scene? scene)
    {
        if (this.Scene == scene) return;
        var old = this.Scene;
        old.DetachFromStage();

        this.Scene = scene ?? _emptyScene;
        this.Scene.AttachToStage(this);

        this.SceneChanged?.Invoke(this, new SceneChangedEventArgs(old, this.Scene));
    }

    /// <summary>
    /// Prepares the stage for rendering. This method should be called by the renderer before rendering the scene.
    /// </summary>
    /// <param name="renderTexture">The render texture that will be used for rendering.</param>
    public void PrepareForRender(RenderTexture renderTexture)
    {
        if (this.MultiSampleCount != renderTexture.SampleCount)
        {
            // This is a hack to invalidate the pipeline of all renderables, but whatever.
            this.MultiSampleCount = renderTexture.SampleCount;
        }

        this.Scene.RenderImGui();

        var camera = this.Scene.Camera;
        if (camera == null) return;

        this.Scene.PrepareForRender();

        var shadowMap = this.Scene.Environment.MainLight.ShadowMap;
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
    /// Updates the stage.
    /// </summary>
    /// <param name="deltaTime">The time since the last update in seconds.</param>
    public virtual void Update(float deltaTime)
    {
        this.Gizmos.Update(deltaTime);
        this.Picking.Update(this.Scene.Camera);
        this.Scene.Update(deltaTime);
    }

    /// <summary>
    /// Notifies the stage that the transform of the specified node is not dirty.
    /// </summary>
    /// <param name="node">The node.</param>
    internal void NotifyTransformNotDirty(Node3D node)
    {
        this._transformDirtyList.Remove(node);
    }

    /// <summary>
    /// Notifies the stage that the transform of the specified node is dirty.
    /// </summary>
    /// <param name="node">The node.</param>
    internal void NotifyTransformDirty(Node3D node)
    {
        this._transformDirtyList.Add(node);
    }

    /// <summary>
    /// Notifies the stage that the renderable pipeline of the specified renderable is dirty.
    /// </summary>
    /// <param name="renderable">The renderable.</param>
    internal void NotifyRenderablePipelineDirty(Renderable renderable)
    {
        this._dirtyRenderables.Add(renderable);
    }

    /// <summary>
    /// Notifies the stage that the renderable queue render flags of the specified renderable have changed.
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
    /// Notifies the stage that the renderable skeleton of the specified renderable has changed.
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

    /// <summary>
    /// Gets the immediate renderables used to render objects immediately.
    /// </summary>
    public IReadOnlyList<ImmediateRenderable3D> ImmediateRenderables => this._immediateRenderables;

    /// <summary>
    /// Adds an immediate renderable to the scene.
    /// </summary>
    /// <param name="immediateRenderable">The immediate renderable to add.</param>
    public void AddImmediateRenderable(ImmediateRenderable3D immediateRenderable)
    {
        this._immediateRenderables.Add(immediateRenderable);
    }

    /// <summary>
    /// Removes an immediate renderable from the scene.
    /// </summary>
    /// <param name="immediateRenderable">The immediate renderable to remove.</param>
    public void RemoveImmediateRenderable(ImmediateRenderable3D immediateRenderable)
    {
        this._immediateRenderables.Remove(immediateRenderable);
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

    private Veldrid.TextureSampleCount _multiSampleCount = Veldrid.TextureSampleCount.Count1;

    /// <summary>
    /// Gets or sets the multi sample count used for rendering.
    /// </summary>
    public Veldrid.TextureSampleCount MultiSampleCount
    {
        get => this._multiSampleCount;
        set => this.SetProperty(ref this._multiSampleCount, value);
    }

    private bool _forceWireframe = false;

    /// <summary>
    /// Gets or sets whether to force wireframe rendering.
    /// </summary>
    public bool ForceWireframe
    {
        get => this._forceWireframe;
        set => this.SetProperty(ref this._forceWireframe, value);
    }

    private bool _enableFog = true;

    /// <summary>
    /// Gets or sets whether to enable fog.
    /// </summary>
    public bool EnableFog
    {
        get => this._enableFog;
        set => this.SetProperty(ref this._enableFog, value);
    }

    private bool _enablePixelPerfectShadows = true;

    /// <summary>
    /// Gets or sets whether to enable pixel perfect shadows.
    /// </summary>
    public bool EnablePixelPerfectShadows
    {
        get => this._enablePixelPerfectShadows;
        set => this.SetProperty(ref this._enablePixelPerfectShadows, value);
    }

    private bool _lightingHalfLambert = true;

    /// <summary>
    /// Gets or sets whether to use half lambert lighting.
    /// </summary>
    public bool LightingHalfLambert
    {
        get => this._lightingHalfLambert;
        set => this.SetProperty(ref this._lightingHalfLambert, value);
    }

    private int _cascadesCount = 4;

    /// <summary>
    /// Gets or sets the number of shadow cascades used for rendering.
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
}
