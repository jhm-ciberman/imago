using System;
using System.Numerics;

namespace LifeSim;

public struct Rectangle
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public Rectangle(Vector2 coords, Vector2 size)
    {
        this.X = coords.X;
        this.Y = coords.Y;
        this.Width = size.X;
        this.Height = size.Y;
    }

    public Rectangle(float x, float y, float width, float height)
    {
        this.X = x;
        this.Y = y;
        this.Width = width;
        this.Height = height;
    }

    public Vector2 Coords => new Vector2(this.X, this.Y);
    public Vector2 Size => new Vector2(this.Width, this.Height);

    public Vector2 Min => new Vector2(this.XMin, this.YMin);
    public Vector2 Max => new Vector2(this.XMax, this.YMax);

    public float XMin { get => MathF.Min(this.X, this.X + this.Width); set { float oldxmax = this.XMax; this.X = value; this.Width = oldxmax - this.X; } }
    public float YMin { get => MathF.Min(this.Y, this.Y + this.Height); set { float oldymax = this.YMax; this.Y = value; this.Height = oldymax - this.Y; } }
    public float XMax { get => MathF.Max(this.X, this.X + this.Width); set { this.Width = value - this.X; } }
    public float YMax { get => MathF.Max(this.Y, this.Y + this.Height); set { this.Height = value - this.Y; } }

    public bool Contains(Vector2 position)
    {
        return position.X >= this.XMin
            && position.Y >= this.YMin
            && position.X < this.XMax
            && position.Y < this.YMax;
    }

    public bool Overlaps(Rectangle other)
    {
        return other.XMin < this.XMax
            && other.XMax > this.XMin
            && other.YMin < this.YMax
            && other.YMax > this.YMin;
    }
}