using System;
using System.Collections.Generic;
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

    internal RenderQueue OpaqueRenderQueue { get; } = new RenderQueue(RenderQueues.Opaque);

    internal RenderQueue TransparentRenderQueue { get; } = new RenderQueue(RenderQueues.Transparent);

    internal RenderQueue[] ShadowCasterRenderQueues { get; } = new RenderQueue[4];

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
        var old = this.Scene;
        old.DetachFromStage();

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
    /// Updates the scene.
    /// </summary>
    /// <param name="deltaTime">The time since the last update in seconds.</param>
    public virtual void Update(float deltaTime)
    {
        this.Gizmos.Update(deltaTime);
        this.Scene.Update(deltaTime);
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
