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

    public void Expand(Vector2 point)
    {
        if (point.X < this.X)
        {
            this.Width += this.X - point.X;
            this.X = point.X;
        }
        else if (point.X > this.Right)
        {
            this.Width = point.X - this.X;
        }

        if (point.Y < this.Y)
        {
            this.Height += this.Y - point.Y;
            this.Y = point.Y;
        }
        else if (point.Y > this.Bottom)
        {
            this.Height = point.Y - this.Y;
        }
    }

    public void Expand(Rect rect)
    {
        if (rect.X < this.X)
        {
            this.Width += this.X - rect.X;
            this.X = rect.X;
        }
        else if (rect.Right > this.Right)
        {
            this.Width = rect.Right - this.X;
        }

        if (rect.Y < this.Y)
        {
            this.Height += this.Y - rect.Y;
            this.Y = rect.Y;
        }
        else if (rect.Bottom > this.Bottom)
        {
            this.Height = rect.Bottom - this.Y;
        }
    }

    public void Transform(Matrix3x2 transform)
    {
        var topLeft = Vector2.Transform(new Vector2(this.X, this.Y), transform);
        var topRight = Vector2.Transform(new Vector2(this.Right, this.Y), transform);
        var bottomLeft = Vector2.Transform(new Vector2(this.X, this.Bottom), transform);
        var bottomRight = Vector2.Transform(new Vector2(this.Right, this.Bottom), transform);

        this.X = MathF.Min(topLeft.X, MathF.Min(topRight.X, MathF.Min(bottomLeft.X, bottomRight.X)));
        this.Y = MathF.Min(topLeft.Y, MathF.Min(topRight.Y, MathF.Min(bottomLeft.Y, bottomRight.Y)));
        this.Width = MathF.Max(topLeft.X, MathF.Max(topRight.X, MathF.Max(bottomLeft.X, bottomRight.X))) - this.X;
        this.Height = MathF.Max(topLeft.Y, MathF.Max(topRight.Y, MathF.Max(bottomLeft.Y, bottomRight.Y))) - this.Y;
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