using System;
using System.Numerics;

namespace LifeSim;

public struct Rect
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public Rect(Vector2 coords, Vector2 size)
    {
        this.X = coords.X;
        this.Y = coords.Y;
        this.Width = size.X;
        this.Height = size.Y;
    }

    public Rect(float x, float y, float width, float height)
    {
        this.X = x;
        this.Y = y;
        this.Width = width;
        this.Height = height;
    }

    public Vector2 Position
    {
        get => new Vector2(this.X, this.Y);
        set
        {
            this.X = value.X;
            this.Y = value.Y;
        }
    }

    public Vector2 Size
    {
        get => new Vector2(this.Width, this.Height);
        set
        {
            this.Width = value.X;
            this.Height = value.Y;
        }
    }

    public float Right { get => this.X + this.Width; set => this.Width = value - this.X; }
    public float Bottom { get => this.Y + this.Height; set => this.Height = value - this.Y; }
    public float Left { get => this.X; set => this.X = value; }
    public float Top { get => this.Y; set => this.Y = value; }


    public bool Contains(Vector2 position)
    {
        return position.X >= this.Left
            && position.Y >= this.Top
            && position.X < this.Right
            && position.Y < this.Bottom;
    }

    public bool Overlaps(Rect other)
    {
        return other.Left < this.Right
            && other.Right > this.Left
            && other.Top < this.Bottom
            && other.Bottom > this.Top;
    }
}