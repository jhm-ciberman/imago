using System;
using System.Numerics;

namespace LifeSim.Imago.SceneGraph;

public class Viewport
{
    /// <summary>
    /// Occurs when the <see cref="Size"/> property changes.
    /// </summary>
    public event EventHandler? SizeChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="Viewport"/> class.
    /// </summary>
    /// <param name="size">The size of the viewport.</param>
    /// <param name="position">The position of the viewport.</param>
    public Viewport(Vector2 size, Vector2 position = default)
    {
        this.Size = size;
        this.Position = position;
    }

    /// <summary>
    /// Resizes the viewport.
    /// </summary>
    /// <param name="size">The new size of the viewport.</param>
    public void Resize(Vector2 size)
    {
        if (this.Size == size) return;

        this.Size = size;
        this.SizeChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Gets the size of the viewport.
    /// </summary>
    public Vector2 Size { get; private set; }

    /// <summary>
    /// Gets the position of the viewport.
    /// </summary>
    public Vector2 Position { get; private set; }

    /// <summary>
    /// Gets the aspect ratio of the viewport.
    /// </summary>
    public float AspectRatio => this.Size.X / this.Size.Y;
}
