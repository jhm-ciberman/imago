using System;

namespace LifeSim.Support.Numerics;

/// <summary>
/// Represents a 2D rectangle with integer coordinates.
/// </summary>
public struct RectInt : IEquatable<RectInt>
{
    /// <summary>
    /// Gets or sets the X position of the rectangle.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Gets or sets the Y position of the rectangle.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Gets or sets the width of the rectangle.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the rectangle.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> struct.
    /// </summary>
    /// <param name="coords">The position of the rectangle.</param>
    /// <param name="size">The size of the rectangle.</param>
    public RectInt(Vector2Int coords, Vector2Int size)
    {
        this.X = coords.X;
        this.Y = coords.Y;
        this.Width = size.X;
        this.Height = size.Y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> struct.
    /// </summary>
    /// <param name="x">The X position of the rectangle.</param>
    /// <param name="y">The Y position of the rectangle.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    public RectInt(int x, int y, int width, int height)
    {
        this.X = x;
        this.Y = y;
        this.Width = width;
        this.Height = height;
    }

    /// <summary>
    /// Gets or sets the position of the rectangle.
    /// </summary>
    public Vector2Int Position
    {
        get => new Vector2Int(this.X, this.Y);
        set
        {
            this.X = value.X;
            this.Y = value.Y;
        }
    }

    /// <summary>
    /// Gets or sets the size of the rectangle.
    /// </summary>
    public Vector2Int Size
    {
        get => new Vector2Int(this.Width, this.Height);
        set
        {
            this.Width = value.X;
            this.Height = value.Y;
        }
    }

    /// <summary>
    /// Get or sets the rightmost position of the rectangle.
    /// </summary>
    public int Right { get => this.X + this.Width; set => this.Width = value - this.X; }

    /// <summary>
    /// Get or sets the bottommost position of the rectangle.
    /// </summary>
    public int Bottom { get => this.Y + this.Height; set => this.Height = value - this.Y; }

    /// <summary>
    /// Get or sets the leftmost position of the rectangle.
    /// </summary>
    public int Left { get => this.X; set => this.X = value; }

    /// <summary>
    /// Get or sets the topmost position of the rectangle.
    /// </summary>
    public int Top { get => this.Y; set => this.Y = value; }

    public Vector2Int Min => new Vector2Int(this.XMin, this.YMin);
    public Vector2Int Max => new Vector2Int(this.XMax, this.YMax);

    public int XMin { get => Math.Min(this.X, this.X + this.Width); set { int oldxmax = this.XMax; this.X = value; this.Width = oldxmax - this.X; } }
    public int YMin { get => Math.Min(this.Y, this.Y + this.Height); set { int oldymax = this.YMax; this.Y = value; this.Height = oldymax - this.Y; } }
    public int XMax { get => Math.Max(this.X, this.X + this.Width); set { this.Width = value - this.X; } }
    public int YMax { get => Math.Max(this.Y, this.Y + this.Height); set { this.Height = value - this.Y; } }


    /// <summary>
    /// Checks if the rectangle contains the specified point.
    /// </summary>
    /// <param name="position">The position to test.</param>
    /// <returns>True if the rectangle contains the point, otherwise false.</returns>
    public bool Contains(Vector2Int position)
    {
        return position.X >= this.XMin
            && position.Y >= this.YMin
            && position.X < this.XMax
            && position.Y < this.YMax;
    }

    /// <summary>
    /// Checks if the rectangle fully contains the specified rectangle.
    /// </summary>
    /// <param name="other">The rectangle to test.</param>
    /// <returns>True if the rectangle contains the other rectangle, otherwise false.</returns>
    public bool Contains(RectInt other)
    {
        return other.XMin >= this.XMin
            && other.YMin >= this.YMin
            && other.XMax <= this.XMax
            && other.YMax <= this.YMax;
    }

    /// <summary>
    /// Checks if the given rectangle overlaps with this rectangle.
    /// </summary>
    /// <param name="other">The rectangle to test.</param>
    /// <returns>True if the rectangles overlap, otherwise false.</returns>
    public bool Overlaps(RectInt other)
    {
        return other.XMin < this.XMax
            && other.XMax > this.XMin
            && other.YMin < this.YMax
            && other.YMax > this.YMin;
    }

    /// <summary>
    /// Checks if the given rectangle is equal to this rectangle.
    /// </summary>
    /// <param name="other">The rectangle to test.</param>
    /// <returns>True if the rectangles are equal, otherwise false.</returns>
    public bool Equals(RectInt other)
    {
        return this.X == other.X
            && this.Y == other.Y
            && this.Width == other.Width
            && this.Height == other.Height;
    }

    /// <summary>
    /// Checks if the given object is equal to this rectangle.
    /// </summary>
    /// <param name="obj">The object to test.</param>
    /// <returns>True if the object is equal to this rectangle, otherwise false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is RectInt other && this.Equals(other);
    }

    /// <summary>
    /// Gets the hash code of this rectangle.
    /// </summary>
    /// <returns>The hash code of this rectangle.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.X, this.Y, this.Width, this.Height);
    }

    /// <summary>
    /// Gets the string representation of this rectangle.
    /// </summary>
    /// <returns>The string representation of this rectangle.</returns>
    public override string ToString()
    {
        return $"RectInt({this.X}, {this.Y}, {this.Width}, {this.Height})";
    }
}
