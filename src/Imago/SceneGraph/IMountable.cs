using System;

namespace Imago.SceneGraph;

/// <summary>
/// Defines the mount lifecycle contract for scene graph elements that support template bindings.
/// </summary>
public interface IMountable
{
    /// <summary>
    /// Occurs when this element has been mounted to the scene graph.
    /// </summary>
    public event EventHandler? Mounted;

    /// <summary>
    /// Occurs when this element is being unmounted from the scene graph.
    /// </summary>
    public event EventHandler? Unmounting;
}
