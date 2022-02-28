using System;

namespace LifeSim;

public struct RectInt
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public RectInt(Vector2Int coords, Vector2Int size)
    {
        this.X = coords.X;
        this.Y = coords.Y;
        this.Width = size.X;
        this.Height = size.Y;
    }

    public RectInt(int x, int y, int width, int height)
    {
        this.X = x;
        this.Y = y;
        this.Width = width;
        this.Height = height;
    }

    public Vector2Int Position => new Vector2Int(this.X, this.Y);
    public Vector2Int Size => new Vector2Int(this.Width, this.Height);

    public Vector2Int Min => new Vector2Int(this.XMin, this.YMin);
    public Vector2Int Max => new Vector2Int(this.XMax, this.YMax);

    public int XMin { get => Math.Min(this.X, this.X + this.Width); set { int oldxmax = this.XMax; this.X = value; this.Width = oldxmax - this.X; } }
    public int YMin { get => Math.Min(this.Y, this.Y + this.Height); set { int oldymax = this.YMax; this.Y = value; this.Height = oldymax - this.Y; } }
    public int XMax { get => Math.Max(this.X, this.X + this.Width); set { this.Width = value - this.X; } }
    public int YMax { get => Math.Max(this.Y, this.Y + this.Height); set { this.Height = value - this.Y; } }

    public bool Contains(Vector2Int position)
    {
        return position.X >= this.XMin
            && position.Y >= this.YMin
            && position.X < this.XMax
            && position.Y < this.YMax;
    }

    public bool Overlaps(RectInt other)
    {
        return other.XMin < this.XMax
            && other.XMax > this.XMin
            && other.YMin < this.YMax
            && other.YMax > this.YMin;
    }
}