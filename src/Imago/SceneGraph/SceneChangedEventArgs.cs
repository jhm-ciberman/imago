using System;

namespace Imago.SceneGraph;

/// <summary>
/// Event arguments for the <see cref="Stage.SceneChanged"/> event.
/// </summary>
public class SceneChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the old scene.
    /// </summary>
    public Scene? OldScene { get; }

    /// <summary>
    /// Gets the new scene.
    /// </summary>
    public Scene? NewScene { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneChangedEventArgs"/> class.
    /// </summary>
    /// <param name="oldScene">The old scene.</param>
    /// <param name="newScene">The new scene.</param>
    public SceneChangedEventArgs(Scene oldScene, Scene newScene)
    {
        this.OldScene = oldScene;
        this.NewScene = newScene;
    }
}
