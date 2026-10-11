using System;
using System.Numerics;

namespace Imago.SceneGraph;

/// <summary>
/// Defines the size of a 2D area that is rendered to. Many objects can share one viewport.
/// </summary>
public class Viewport
{
    /// <summary>
    /// Occurs when the <see cref="Size"/> property changes.
    /// </summary>
    public event EventHandler? SizeChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="Viewport"/> class.
    /// </summary>
    /// <param name="size">The size of the viewport in pixels.</param>
    public Viewport(Vector2 size)
    {
        this.Size = size;
    }

    /// <summary>
    /// Resizes the viewport.
    /// </summary>
    /// <param name="size">The new size of the viewport in pixels.</param>
    public void Resize(Vector2 size)
    {
        if (this.Size == size) return;

        this.Size = size;
        this.SizeChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Gets the size of the viewport in pixels.
    /// </summary>
    public Vector2 Size { get; private set; }

    /// <summary>
    /// Gets the aspect ratio of the viewport.
    /// </summary>
    public float AspectRatio => this.Size.X / this.Size.Y;
}
