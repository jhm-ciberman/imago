using System;
using System.Collections.Generic;
using LifeSim.Imago.Input;
using LifeSim.Imago.Rendering;
using LifeSim.Imago.SceneGraph.Nodes;
using LifeSim.Imago.SceneGraph.Picking;
using LifeSim.Imago.Textures;

namespace LifeSim.Imago.SceneGraph;

/// <summary>
/// Represents the main container for a <see cref="Scene"/> and manages the overall rendering state.
/// </summary>
/// <remarks>
/// The Stage is responsible for holding the active scene, managing render queues, and handling the update and render loop.
/// It also provides properties to control various rendering features like wireframe mode, fog, and shadows.
/// </remarks>
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
    /// Gets the gizmos layer used to render debug and editor visuals.
    /// </summary>
    public GizmosLayer Gizmos { get; } = new GizmosLayer();

    /// <summary>
    /// Gets the picking manager for object selection in the scene.
    /// </summary>
    public PickingManager Picking { get; } = PickingManager.Instance;

    /// <summary>
    /// Gets the currently active scene.
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
    /// Subscribes to input events to enable input handling for the current scene.
    /// </summary>
    public void EnableInputHandling()
    {
        var input = InputManager.Instance;
        input.MouseButtonPressed += this.Input_MouseButtonPressed;
        input.MouseButtonReleased += this.Input_MouseButtonReleased;
        input.MouseWheelScrolled += this.Input_MouseWheelScrolled;
        input.KeyPressed += this.Input_KeyPressed;
        input.KeyReleased += this.Input_KeyReleased;
    }

    /// <summary>
    /// Unsubscribes from input events to disable input handling for the current scene.
    /// </summary>
    public void DisableInputHandling()
    {
        var input = InputManager.Instance;
        input.MouseButtonPressed -= this.Input_MouseButtonPressed;
        input.MouseButtonReleased -= this.Input_MouseButtonReleased;
        input.MouseWheelScrolled -= this.Input_MouseWheelScrolled;
        input.KeyPressed -= this.Input_KeyPressed;
        input.KeyReleased -= this.Input_KeyReleased;
    }

    private void Input_MouseButtonPressed(object? sender, MouseButtonEventArgs e)
    {
        this.Scene.HandleMousePressed(e);
    }

    private void Input_MouseButtonReleased(object? sender, MouseButtonEventArgs e)
    {
        this.Scene.HandleMouseReleased(e);
    }

    private void Input_MouseWheelScrolled(object? sender, MouseWheelEventArgs e)
    {
        this.Scene.HandleMouseWheelScrolled(e);
    }

    private void Input_KeyPressed(object? sender, KeyboardEventArgs e)
    {
        this.Scene.HandleKeyPressed(e);
    }

    private void Input_KeyReleased(object? sender, KeyboardEventArgs e)
    {
        this.Scene.HandleKeyReleased(e);
    }

    /// <summary>
    /// Changes the currently active scene, detaching the old one and attaching the new one.
    /// </summary>
    /// <param name="scene">The new scene to display. If null, an empty scene is used.</param>
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
    /// Prepares the stage for rendering by updating transforms, skeletons, and render queues.
    /// </summary>
    /// <remarks>
    /// This method should be called by the renderer before executing the main render passes.
    /// </remarks>
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
    /// Updates the stage and the active scene's logic.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update, in seconds.</param>
    public virtual void Update(float deltaTime)
    {
        this.Gizmos.Update(deltaTime);

        bool isCursorOverUi = this.Scene.GuiLayer?.IsCursorOverElement ?? false;
        this.Picking.Update(this.Scene.Camera, isCursorOverUi);
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
    /// Gets the list of immediate-mode renderables.
    /// </summary>
    public IReadOnlyList<ImmediateRenderable3D> ImmediateRenderables => this._immediateRenderables;

    /// <summary>
    /// Adds an immediate-mode renderable to the stage.
    /// </summary>
    /// <param name="immediateRenderable">The immediate renderable to add.</param>
    public void AddImmediateRenderable(ImmediateRenderable3D immediateRenderable)
    {
        this._immediateRenderables.Add(immediateRenderable);
    }

    /// <summary>
    /// Removes an immediate-mode renderable from the stage.
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
    /// Gets or sets the multi-sample anti-aliasing (MSAA) count for rendering.
    /// </summary>
    public Veldrid.TextureSampleCount MultiSampleCount
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
}
