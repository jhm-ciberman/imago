using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace LifeSim.Imago.Utilities;

/// <summary>
/// Represents an axis-aligned bounding box in 3D space.
/// </summary>
public struct BoundingBox : IEquatable<BoundingBox>
{
    /// <summary>
    /// The minimum point of the bounding box.
    /// </summary>
    public Vector3 Min;

    /// <summary>
    /// The maximum point of the bounding box.
    /// </summary>
    public Vector3 Max;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundingBox"/> struct.
    /// </summary>
    /// <param name="min">The minimum point of the bounding box.</param>
    /// <param name="max">The maximum point of the bounding box.</param>
    public BoundingBox(Vector3 min, Vector3 max)
    {
        this.Min = min;
        this.Max = max;
    }

    /// <summary>
    /// Determines how another bounding box is contained within this one.
    /// </summary>
    /// <param name="other">The other bounding box to check.</param>
    /// <returns>A <see cref="ContainmentType"/> indicating the relationship between the two boxes.</returns>
    public ContainmentType Contains(ref BoundingBox other)
    {
        if (this.Max.X < other.Min.X || this.Min.X > other.Max.X
            || this.Max.Y < other.Min.Y || this.Min.Y > other.Max.Y
            || this.Max.Z < other.Min.Z || this.Min.Z > other.Max.Z)
        {
            return ContainmentType.Disjoint;
        }
        else if (this.Min.X <= other.Min.X && this.Max.X >= other.Max.X
            && this.Min.Y <= other.Min.Y && this.Max.Y >= other.Max.Y
            && this.Min.Z <= other.Min.Z && this.Max.Z >= other.Max.Z)
        {
            return ContainmentType.Contains;
        }
        else
        {
            return ContainmentType.Intersects;
        }
    }

    /// <summary>
    /// Gets the center point of the bounding box.
    /// </summary>
    /// <returns>The center point.</returns>
    public Vector3 GetCenter()
    {
        return (this.Max + this.Min) / 2f;
    }

    /// <summary>
    /// Gets the dimensions (width, height, depth) of the bounding box.
    /// </summary>
    /// <returns>The dimensions of the box.</returns>
    public Vector3 GetDimensions()
    {
        return this.Max - this.Min;
    }

    /// <summary>
    /// Creates a new bounding box that is transformed by the given matrix.
    /// </summary>
    /// <param name="box">The original bounding box.</param>
    /// <param name="mat">The transformation matrix.</param>
    /// <returns>The transformed bounding box.</returns>
    public static unsafe BoundingBox Transform(BoundingBox box, Matrix4x4 mat)
    {
        AlignedBoxCorners corners = box.GetCorners();
        Vector3* cornersPtr = (Vector3*)&corners;

        Vector3 min = Vector3.Transform(cornersPtr[0], mat);
        Vector3 max = Vector3.Transform(cornersPtr[0], mat);

        for (int i = 1; i < 8; i++)
        {
            min = Vector3.Min(min, Vector3.Transform(cornersPtr[i], mat));
            max = Vector3.Max(max, Vector3.Transform(cornersPtr[i], mat));
        }

        return new BoundingBox(min, max);
    }

    /// <summary>
    /// Creates a new bounding box from a set of vertices.
    /// </summary>
    /// <param name="vertices">A pointer to the first vertex.</param>
    /// <param name="numVertices">The number of vertices.</param>
    /// <param name="rotation">A rotation to apply to the vertices.</param>
    /// <param name="offset">An offset to apply to the vertices.</param>
    /// <param name="scale">A scale to apply to the vertices.</param>
    /// <returns>The created bounding box.</returns>
    public static unsafe BoundingBox CreateFromVertices(
        Vector3* vertices,
        int numVertices,
        Quaternion rotation,
        Vector3 offset,
        Vector3 scale)
        => CreateFromPoints(vertices, Unsafe.SizeOf<Vector3>(), numVertices, rotation, offset, scale);

    /// <summary>
    /// Creates a new bounding box from a set of points.
    /// </summary>
    /// <param name="vertexPtr">A pointer to the first point.</param>
    /// <param name="numVertices">The number of points.</param>
    /// <param name="vertexStride">The stride between points in bytes.</param>
    /// <param name="rotation">A rotation to apply to the points.</param>
    /// <param name="offset">An offset to apply to the points.</param>
    /// <param name="scale">A scale to apply to the points.</param>
    /// <returns>The created bounding box.</returns>
    public static unsafe BoundingBox CreateFromPoints(
        Vector3* vertexPtr,
        int numVertices,
        int vertexStride,
        Quaternion rotation,
        Vector3 offset,
        Vector3 scale)
    {
        byte* bytePtr = (byte*)vertexPtr;
        Vector3 min = Vector3.Transform(*vertexPtr, rotation);
        Vector3 max = Vector3.Transform(*vertexPtr, rotation);

        for (int i = 1; i < numVertices; i++)
        {
            bytePtr = bytePtr + vertexStride;
            vertexPtr = (Vector3*)bytePtr;
            Vector3 pos = Vector3.Transform(*vertexPtr, rotation);

            if (min.X > pos.X) min.X = pos.X;
            if (max.X < pos.X) max.X = pos.X;

            if (min.Y > pos.Y) min.Y = pos.Y;
            if (max.Y < pos.Y) max.Y = pos.Y;

            if (min.Z > pos.Z) min.Z = pos.Z;
            if (max.Z < pos.Z) max.Z = pos.Z;
        }

        return new BoundingBox(min * scale + offset, max * scale + offset);
    }

    /// <summary>
    /// Creates a new bounding box from an array of vertices.
    /// </summary>
    /// <param name="vertices">The array of vertices.</param>
    /// <returns>The created bounding box.</returns>
    public static unsafe BoundingBox CreateFromVertices(Vector3[] vertices)
    {
        return CreateFromVertices(vertices, Quaternion.Identity, Vector3.Zero, Vector3.One);
    }

    /// <summary>
    /// Creates a new bounding box from an array of vertices with a transformation.
    /// </summary>
    /// <param name="vertices">The array of vertices.</param>
    /// <param name="rotation">A rotation to apply to the vertices.</param>
    /// <param name="offset">An offset to apply to the vertices.</param>
    /// <param name="scale">A scale to apply to the vertices.</param>
    /// <returns>The created bounding box.</returns>
    public static unsafe BoundingBox CreateFromVertices(Vector3[] vertices, Quaternion rotation, Vector3 offset, Vector3 scale)
    {
        Vector3 min = Vector3.Transform(vertices[0], rotation);
        Vector3 max = Vector3.Transform(vertices[0], rotation);

        for (int i = 1; i < vertices.Length; i++)
        {
            Vector3 pos = Vector3.Transform(vertices[i], rotation);

            if (min.X > pos.X) min.X = pos.X;
            if (max.X < pos.X) max.X = pos.X;

            if (min.Y > pos.Y) min.Y = pos.Y;
            if (max.Y < pos.Y) max.Y = pos.Y;

            if (min.Z > pos.Z) min.Z = pos.Z;
            if (max.Z < pos.Z) max.Z = pos.Z;
        }

        return new BoundingBox(min * scale + offset, max * scale + offset);
    }

    /// <summary>
    /// Creates a new bounding box that contains two other bounding boxes.
    /// </summary>
    /// <param name="box1">The first bounding box.</param>
    /// <param name="box2">The second bounding box.</param>
    /// <returns>A new bounding box that contains both input boxes.</returns>
    public static BoundingBox Combine(BoundingBox box1, BoundingBox box2)
    {
        return new BoundingBox(
            Vector3.Min(box1.Min, box2.Min),
            Vector3.Max(box1.Max, box2.Max));
    }

    public static bool operator ==(BoundingBox first, BoundingBox second)
    {
        return first.Equals(second);
    }

    public static bool operator !=(BoundingBox first, BoundingBox second)
    {
        return !first.Equals(second);
    }

    public bool Equals(BoundingBox other)
    {
        return this.Min == other.Min && this.Max == other.Max;
    }

    public override string ToString()
    {
        return string.Format("Min:{0}, Max:{1}", this.Min, this.Max);
    }

    public override bool Equals(object? obj)
    {
        return obj is BoundingBox && ((BoundingBox)obj).Equals(this);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Min, this.Max);
    }

    /// <summary>
    /// Gets the eight corners of the bounding box.
    /// </summary>
    /// <returns>An <see cref="AlignedBoxCorners"/> struct containing the corner points.</returns>
    public AlignedBoxCorners GetCorners()
    {
        AlignedBoxCorners corners;
        corners.NearBottomLeft = new Vector3(this.Min.X, this.Min.Y, this.Max.Z);
        corners.NearBottomRight = new Vector3(this.Max.X, this.Min.Y, this.Max.Z);
        corners.NearTopLeft = new Vector3(this.Min.X, this.Max.Y, this.Max.Z);
        corners.NearTopRight = new Vector3(this.Max.X, this.Max.Y, this.Max.Z);

        corners.FarBottomLeft = new Vector3(this.Min.X, this.Min.Y, this.Min.Z);
        corners.FarBottomRight = new Vector3(this.Max.X, this.Min.Y, this.Min.Z);
        corners.FarTopLeft = new Vector3(this.Min.X, this.Max.Y, this.Min.Z);
        corners.FarTopRight = new Vector3(this.Max.X, this.Max.Y, this.Min.Z);

        return corners;
    }

    /// <summary>
    /// Checks if any of the bounding box's components are NaN.
    /// </summary>
    /// <returns>True if any component is NaN; otherwise, false.</returns>
    public bool ContainsNaN()
    {
        return float.IsNaN(this.Min.X) || float.IsNaN(this.Min.Y) || float.IsNaN(this.Min.Z)
            || float.IsNaN(this.Max.X) || float.IsNaN(this.Max.Y) || float.IsNaN(this.Max.Z);
    }

}
