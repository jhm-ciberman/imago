using System;

namespace LifeSim.Imago.SceneGraph;

/// <summary>
/// Event arguments for layer added/removed events.
/// </summary>
public class LayerChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the layer that was added or removed.
    /// </summary>
    public ILayer Layer { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LayerChangedEventArgs"/> class.
    /// </summary>
    /// <param name="layer">The layer that was added or removed.</param>
    public LayerChangedEventArgs(ILayer layer)
    {
        this.Layer = layer;
    }
}
