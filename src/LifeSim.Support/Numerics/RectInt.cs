using System;

namespace LifeSim.Support.Numerics;

/// <summary>
/// Represents a 2D rectangle with integer coordinates.
/// </summary>
public struct RectInt : IEquatable<RectInt>
{
    /// <summary>
    /// Gets an empty rectangle with all properties set to zero.
    /// </summary>
    public static RectInt Empty => default;

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
    /// Creates a rectangle from two corners.
    /// </summary>
    /// <param name="corner1">The first corner of the rectangle.</param>
    /// <param name="corner2">The second corner of the rectangle.</param>
    /// <returns>A new rectangle defined by the two corners.</returns>
    public static RectInt FromCorners(Vector2Int corner1, Vector2Int corner2)
    {
        int x = Math.Min(corner1.X, corner2.X);
        int y = Math.Min(corner1.Y, corner2.Y);
        int width = Math.Abs(corner2.X - corner1.X);
        int height = Math.Abs(corner2.Y - corner1.Y);
        return new RectInt(x, y, width, height);
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
    public int Left
    {
        get => this.X;
        set
        {
            int oldX = this.X;
            this.X = value;
            this.Width = oldX + this.Width - this.X;
        }
    }

    /// <summary>
    /// Get or sets the topmost position of the rectangle.
    /// </summary>
    public int Top
    {
        get => this.Y;
        set
        {
            int oldY = this.Y;
            this.Y = value;
            this.Height = oldY + this.Height - this.Y;
        }
    }

    /// <summary>
    /// Gets the minimum corner of the rectangle.
    /// </summary>
    public Vector2Int Min => new Vector2Int(this.XMin, this.YMin);

    /// <summary>
    /// Gets the maximum corner of the rectangle.
    /// </summary>
    public Vector2Int Max => new Vector2Int(this.XMax, this.YMax);

    /// <summary>
    /// Gets or sets the minimum X coordinate of the rectangle.
    /// </summary>
    public int XMin { get => Math.Min(this.X, this.X + this.Width); set { int oldxmax = this.XMax; this.X = value; this.Width = oldxmax - this.X; } }

    /// <summary>
    /// Gets or sets the minimum Y coordinate of the rectangle.
    /// </summary>
    public int YMin { get => Math.Min(this.Y, this.Y + this.Height); set { int oldymax = this.YMax; this.Y = value; this.Height = oldymax - this.Y; } }

    /// <summary>
    /// Gets or sets the maximum X coordinate of the rectangle.
    /// </summary>
    public int XMax { get => Math.Max(this.X, this.X + this.Width); set { this.Width = value - this.X; } }

    /// <summary>
    /// Gets or sets the maximum Y coordinate of the rectangle.
    /// </summary>
    public int YMax { get => Math.Max(this.Y, this.Y + this.Height); set { this.Height = value - this.Y; } }

    /// <summary>
    /// Gets a value indicating whether this rectangle is empty (has zero or negative area).
    /// </summary>
    public bool IsEmpty => this.Width <= 0 || this.Height <= 0;


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
    /// Deflates the rectangle by the specified padding.
    /// </summary>
    /// <param name="padding">The padding to deflate the rectangle by.</param>
    /// <returns>The deflated rectangle.</returns>
    public RectInt Deflate(int padding)
    {
        return this.Deflate(new Vector2Int(padding, padding));
    }

    /// <summary>
    /// Deflates the rectangle by the specified padding.
    /// </summary>
    /// <param name="padding">The padding to deflate the rectangle by.</param>
    /// <returns>The deflated rectangle.</returns>
    public RectInt Deflate(Vector2Int padding)
    {
        return new RectInt(this.Position + padding, this.Size - padding * 2);
    }

    /// <summary>
    /// Deflates the rectangle by the specified padding.
    /// </summary>
    /// <param name="padding">The padding to deflate the rectangle by.</param>
    /// <returns>The deflated rectangle.</returns>
    public RectInt Deflate(ThicknessInt padding)
    {
        return new RectInt(this.Position + padding.TopLeft, this.Size - padding.Total);
    }

    /// <summary>
    /// Inflates the rectangle by the specified padding.
    /// </summary>
    /// <param name="padding">The padding to inflate the rectangle by.</param>
    /// <returns>The inflated rectangle.</returns>
    public RectInt Inflate(int padding)
    {
        return this.Inflate(new Vector2Int(padding, padding));
    }

    /// <summary>
    /// Inflates the rectangle by the specified padding.
    /// </summary>
    /// <param name="padding">The padding to inflate the rectangle by.</param>
    /// <returns>The inflated rectangle.</returns>
    public RectInt Inflate(Vector2Int padding)
    {
        return new RectInt(this.Position - padding, this.Size + padding * 2);
    }

    /// <summary>
    /// Inflates the rectangle by the specified padding.
    /// </summary>
    /// <param name="padding">The padding to inflate the rectangle by.</param>
    /// <returns>The inflated rectangle.</returns>
    public RectInt Inflate(ThicknessInt padding)
    {
        return new RectInt(this.Position - padding.TopLeft, this.Size + padding.Total);
    }

    /// <summary>
    /// Checks if this rectangle intersects with the given rectangle.
    /// </summary>
    /// <param name="bounds">The rectangle to test.</param>
    /// <param name="intersection">The intersection rectangle.</param>
    /// <returns>True if the rectangles intersect, otherwise false.</returns>
    public bool IntersectionTest(RectInt bounds, out RectInt intersection)
    {
        // Calculate the intersection rectangle
        int xMin = Math.Max(this.X, bounds.X);
        int yMin = Math.Max(this.Y, bounds.Y);
        int xMax = Math.Min(this.X + this.Width, bounds.X + bounds.Width);
        int yMax = Math.Min(this.Y + this.Height, bounds.Y + bounds.Height);

        // Check if there is an intersection
        if (xMin < xMax && yMin < yMax)
        {
            intersection = new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
            return true;
        }

        intersection = default;
        return false;
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
