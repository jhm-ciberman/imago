using System;
using System.Numerics;

namespace Imago.SceneGraph;

public class Viewport
{
    /// <summary>
    /// Occurs when the viewport is resized.
    /// </summary>
    public event EventHandler? Resized;

    /// <summary>
    /// Initializes a new instance of the <see cref="Viewport"/> class.
    /// </summary>
    /// <param name="width">The width of the viewport.</param>
    /// <param name="height">The height of the viewport.</param>
    public Viewport(uint width, uint height) : this(0, 0, width, height) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Viewport"/> class.
    /// </summary>
    /// <param name="x">The x-coordinate of the viewport.</param>
    /// <param name="y">The y-coordinate of the viewport.</param>
    /// <param name="width">The width of the viewport.</param>
    /// <param name="height">The height of the viewport.</param>
    public Viewport(uint x, uint y, uint width, uint height)
    {
        this.X = x;
        this.Y = y;
        this.Width = width;
        this.Height = height;
    }

    /// <summary>
    /// Resizes the viewport.
    /// </summary>
    /// <param name="width">The new width of the viewport.</param>
    /// <param name="height">The new height of the viewport.</param>
    public void Resize(uint width, uint height)
    {
        if (this.Width == width && this.Height == height)
            return;
        this.Width = width;
        this.Height = height;
        this.Resized?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Gets the width of the viewport.
    /// </summary>
    public uint Width { get; private set; }

    /// <summary>
    /// Gets the height of the viewport.
    /// </summary>
    public uint Height { get; private set; }

    /// <summary>
    /// Gets the x-coordinate of the viewport.
    /// </summary>
    public uint X { get; private set; }

    /// <summary>
    /// Gets the y-coordinate of the viewport.
    /// </summary>
    public uint Y { get; private set; }

    /// <summary>
    /// Gets the aspect ratio of the viewport.
    /// </summary>
    public float AspectRatio => this.Width / (float)this.Height;

    /// <summary>
    /// Gets the size of the viewport as a <see cref="Vector2"/>.
    /// </summary>
    public Vector2 Size => new Vector2(this.Width, this.Height);
}
