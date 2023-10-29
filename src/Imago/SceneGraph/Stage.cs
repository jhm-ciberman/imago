using System;
using System.Collections.Generic;
using Imago.Rendering;
using Imago.Rendering.Forward;

namespace Imago.SceneGraph;

public class SceneChangedEventArgs : EventArgs
{
    public Scene? OldScene { get; }

    public Scene? NewScene { get; }

    public SceneChangedEventArgs(Scene oldScene, Scene newScene)
    {
        this.OldScene = oldScene;
        this.NewScene = newScene;
    }
}

public class Stage
{
    /// <summary>
    /// Occurs when the current <see cref="Scene"/> changes.
    /// </summary>
    public event EventHandler<SceneChangedEventArgs>? SceneChanged;

    private class EmptyScene : Scene { }

    private static readonly Scene _emptyScene = new EmptyScene();

    /// <summary>
    /// Gets the gizmos layer used to render gizmos.
    /// </summary>
    public GizmosLayer Gizmos { get; } = new GizmosLayer();

    /// <summary>
    /// Gets the picking manager used to pick objects.
    /// </summary>
    public PickingManger Picking { get; } = new PickingManger();

    /// <summary>
    /// Gets the current scene.
    /// </summary>
    public Scene Scene { get; private set; } = _emptyScene;

    private readonly List<Node3D> _transformDirtyList = new List<Node3D>();

    internal RenderQueue PickingRenderQueue { get; } = new RenderQueue(RenderQueues.Picking);

    internal RenderQueue OpaqueRenderQueue { get; } = new RenderQueue(RenderQueues.Opaque);

    internal RenderQueue TransparentRenderQueue { get; } = new RenderQueue(RenderQueues.Transparent);

    internal RenderQueue[] ShadowCasterRenderQueues { get; } = new RenderQueue[4];

    private readonly List<Renderable> _dirtyRenderables = new();
    private readonly List<ImmediateRenderable3D> _immediateRenderables = new();

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
    /// <param name="disposeOld">if set to <c>true</c> the old scene will be disposed.</param>
    public void ChangeScene(Scene? scene, bool disposeOld = true)
    {
        var old = this.Scene;
        old.DetachFromStage();

        if (disposeOld && old != _emptyScene) old.Dispose();

        this.Scene = scene ?? _emptyScene;
        this.Scene.AttachToStage(this);

        this.SceneChanged?.Invoke(this, new SceneChangedEventArgs(old, this.Scene));
    }

    /// <summary>
    /// Prepares the stage for rendering. This method should be called before rendering a new frame.
    /// </summary>
    public void PrepareForRender()
    {
        this.Scene.PrepareForRender();
        this.Scene.RenderImGui();

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
                renderable.Update(Renderer.Instance);
            }
            this._dirtyRenderables.Clear();
        }
    }

    /// <summary>
    /// Updates the stage.
    /// </summary>
    /// <param name="deltaTime">The time since the last update in seconds.</param>
    public virtual void Update(float deltaTime)
    {
        this.Gizmos.Update(deltaTime);
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
}
