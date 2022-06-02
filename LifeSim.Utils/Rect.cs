using System;
using System.Numerics;

namespace LifeSim;

public struct Rect : IEquatable<Rect>
{
    public static Rect Empty { get; } = new Rect(0, 0, 0, 0);

    public static Rect Infinite { get; } = new Rect(float.NegativeInfinity, float.NegativeInfinity, float.PositiveInfinity, float.PositiveInfinity);

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

    public bool Equals(Rect other)
    {
        return this.X == other.X
            && this.Y == other.Y
            && this.Width == other.Width
            && this.Height == other.Height;
    }

    public override bool Equals(object? obj)
    {
        return obj is Rect other && this.Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.X, this.Y, this.Width, this.Height);
    }

    public override string ToString()
    {
        return $"{this.X}, {this.Y}, {this.Width}, {this.Height}";
    }

    public static bool operator ==(Rect left, Rect right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Rect left, Rect right)
    {
        return !(left == right);
    }

    public static Rect operator *(Rect rect, float scale)
    {
        return new Rect(rect.X * scale, rect.Y * scale, rect.Width * scale, rect.Height * scale);
    }

    public static Rect operator /(Rect rect, float scale)
    {
        return new Rect(rect.X / scale, rect.Y / scale, rect.Width / scale, rect.Height / scale);
    }
}