using System;
using System.Numerics;

namespace LifeSim.Engine.Rendering;

public class Viewport
{
    /// <summary>
    /// Raised when the viewport is resized.
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

    public void Resize(uint width, uint height)
    {
        if (this.Width == width && this.Height == height)
        {
            return;
        }
        this.Width = width;
        this.Height = height;
        this.Resized?.Invoke(this, EventArgs.Empty);
    }

    public void Move(uint x, uint y)
    {
        this.X = x;
        this.Y = y;
    }

    public uint Width { get; private set; }
    public uint Height { get; private set; }
    public uint X { get; private set; }
    public uint Y { get; private set; }
    public Vector2 Size => new Vector2(this.Width, this.Height);
}