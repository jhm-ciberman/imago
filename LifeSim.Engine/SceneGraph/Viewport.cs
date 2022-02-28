using System.Numerics;

namespace LifeSim.Engine.Rendering;

public class Viewport
{
    public event System.Action<Viewport>? OnResized;

    public Viewport(uint width, uint height) : this(0, 0, width, height) { }

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
        this.OnResized?.Invoke(this);
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