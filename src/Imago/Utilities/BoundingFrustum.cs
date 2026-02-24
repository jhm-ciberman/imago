using System.Numerics;
using System.Runtime.CompilerServices;

namespace Imago.Utilities;

/// <summary>
/// Represents a view frustum, defined by six planes, used for visibility culling.
/// </summary>
public unsafe struct BoundingFrustum
{
    private SixPlane _planes;

    private struct SixPlane
    {
        public Plane Left;
        public Plane Right;
        public Plane Bottom;
        public Plane Top;
        public Plane Near;
        public Plane Far;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundingFrustum"/> struct from a combined view-projection matrix.
    /// </summary>
    /// <param name="m">The view-projection matrix.</param>
    public BoundingFrustum(Matrix4x4 m)
    {
        // Plane computations: http://gamedevs.org/uploads/fast-extraction-viewing-frustum-planes-from-world-view-projection-matrix.pdf
        this._planes.Left = Plane.Normalize(
            new Plane(
                m.M14 + m.M11,
                m.M24 + m.M21,
                m.M34 + m.M31,
                m.M44 + m.M41));

        this._planes.Right = Plane.Normalize(
            new Plane(
                m.M14 - m.M11,
                m.M24 - m.M21,
                m.M34 - m.M31,
                m.M44 - m.M41));

        this._planes.Bottom = Plane.Normalize(
            new Plane(
                m.M14 + m.M12,
                m.M24 + m.M22,
                m.M34 + m.M32,
                m.M44 + m.M42));

        this._planes.Top = Plane.Normalize(
            new Plane(
                m.M14 - m.M12,
                m.M24 - m.M22,
                m.M34 - m.M32,
                m.M44 - m.M42));

        this._planes.Near = Plane.Normalize(
            new Plane(
                m.M13,
                m.M23,
                m.M33,
                m.M43));

        this._planes.Far = Plane.Normalize(
            new Plane(
                m.M14 - m.M13,
                m.M24 - m.M23,
                m.M34 - m.M33,
                m.M44 - m.M43));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundingFrustum"/> struct from six planes.
    /// </summary>
    /// <param name="left">The left plane.</param>
    /// <param name="right">The right plane.</param>
    /// <param name="bottom">The bottom plane.</param>
    /// <param name="top">The top plane.</param>
    /// <param name="near">The near plane.</param>
    /// <param name="far">The far plane.</param>
    public BoundingFrustum(Plane left, Plane right, Plane bottom, Plane top, Plane near, Plane far)
    {
        this._planes.Left = left;
        this._planes.Right = right;
        this._planes.Bottom = bottom;
        this._planes.Top = top;
        this._planes.Near = near;
        this._planes.Far = far;
    }

    /// <summary>
    /// Checks whether the frustum contains the specified point.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <returns>A <see cref="ContainmentType"/> indicating the relationship.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ContainmentType Contains(Vector3 point)
    {
        Plane* planes = (Plane*)Unsafe.AsPointer(ref this._planes); // Is this safe?

        for (int i = 0; i < 6; i++)
        {
            if (Plane.DotCoordinate(planes[i], point) < 0)
                return ContainmentType.Disjoint;
        }

        return ContainmentType.Contains;
    }

    /// <summary>
    /// Checks whether the frustum contains the specified point.
    /// </summary>
    /// <param name="point">A pointer to the point to check.</param>
    /// <returns>A <see cref="ContainmentType"/> indicating the relationship.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ContainmentType Contains(Vector3* point)
    {
        Plane* planes = (Plane*)Unsafe.AsPointer(ref this._planes); // Is this safe?

        for (int i = 0; i < 6; i++)
        {
            if (Plane.DotCoordinate(planes[i], *point) < 0)
                return ContainmentType.Disjoint;
        }

        return ContainmentType.Contains;
    }

    /// <summary>
    /// Checks whether the frustum contains the specified bounding sphere.
    /// </summary>
    /// <param name="sphere">The bounding sphere to check.</param>
    /// <returns>A <see cref="ContainmentType"/> indicating the relationship.</returns>
    public ContainmentType Contains(BoundingSphere sphere)
    {
        Plane* planes = (Plane*)Unsafe.AsPointer(ref this._planes);

        ContainmentType result = ContainmentType.Contains;
        for (int i = 0; i < 6; i++)
        {
            float distance = Plane.DotCoordinate(planes[i], sphere.Center);
            if (distance < -sphere.Radius)
            {
                return ContainmentType.Disjoint;
            }
            else if (distance < sphere.Radius)
            {
                result = ContainmentType.Intersects;
            }
        }

        return result;
    }

    /// <summary>
    /// Checks whether the frustum contains the specified bounding box.
    /// </summary>
    /// <param name="box">The bounding box to check.</param>
    /// <returns>A <see cref="ContainmentType"/> indicating the relationship.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ContainmentType Contains(BoundingBox box) => this.Contains(ref box);

    /// <summary>
    /// Checks whether the frustum contains the specified bounding box.
    /// </summary>
    /// <param name="box">The bounding box to check.</param>
    /// <returns>A <see cref="ContainmentType"/> indicating the relationship.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ContainmentType Contains(ref BoundingBox box)
    {
        Plane* planes = (Plane*)Unsafe.AsPointer(ref this._planes);

        ContainmentType result = ContainmentType.Contains;
        for (int i = 0; i < 6; i++)
        {
            Plane plane = planes[i];

            // Approach: http://zach.in.tu-clausthal.de/teaching/cg_literatur/lighthouse3d_view_frustum_culling/index.html

            Vector3 positive = new Vector3(box.Min.X, box.Min.Y, box.Min.Z);
            Vector3 negative = new Vector3(box.Max.X, box.Max.Y, box.Max.Z);

            if (plane.Normal.X >= 0)
            {
                positive.X = box.Max.X;
                negative.X = box.Min.X;
            }
            if (plane.Normal.Y >= 0)
            {
                positive.Y = box.Max.Y;
                negative.Y = box.Min.Y;
            }
            if (plane.Normal.Z >= 0)
            {
                positive.Z = box.Max.Z;
                negative.Z = box.Min.Z;
            }

            // If the positive vertex is outside (behind plane), the box is disjoint.
            float positiveDistance = Plane.DotCoordinate(plane, positive);
            if (positiveDistance < 0)
                return ContainmentType.Disjoint;

            // If the negative vertex is outside (behind plane), the box is intersecting.
            // Because the above check failed, the positive vertex is in front of the plane,
            // and the negative vertex is behind. Thus, the box is intersecting this plane.
            float negativeDistance = Plane.DotCoordinate(plane, negative);
            if (negativeDistance < 0)
                result = ContainmentType.Intersects;
        }

        return result;
    }

    /// <summary>
    /// Checks whether the frustum contains another frustum.
    /// </summary>
    /// <param name="other">The other frustum to check.</param>
    /// <returns>A <see cref="ContainmentType"/> indicating the relationship.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ContainmentType Contains(ref BoundingFrustum other)
    {
        int pointsContained = 0;
        FrustumCorners corners = other.GetCorners();
        Vector3* cornersPtr = (Vector3*)&corners;
        for (int i = 0; i < 8; i++)
        {
            if (this.Contains(&cornersPtr[i]) != ContainmentType.Disjoint)
                pointsContained++;
        }

        if (pointsContained == 8)
        {
            return ContainmentType.Contains;
        }
        else if (pointsContained == 0)
        {
            return ContainmentType.Disjoint;
        }
        else
        {
            return ContainmentType.Intersects;
        }
    }

    /// <summary>
    /// Gets the eight corners of the frustum.
    /// </summary>
    /// <returns>A <see cref="FrustumCorners"/> struct containing the corner points.</returns>
    public FrustumCorners GetCorners()
    {
        this.GetCorners(out FrustumCorners corners);
        return corners;
    }

    /// <summary>
    /// Gets the eight corners of the frustum.
    /// </summary>
    /// <param name="corners">An out parameter that will be populated with the corner points.</param>
    public void GetCorners(out FrustumCorners corners)
    {
        PlaneIntersection(ref this._planes.Near, ref this._planes.Top, ref this._planes.Left, out corners.NearTopLeft);
        PlaneIntersection(ref this._planes.Near, ref this._planes.Top, ref this._planes.Right, out corners.NearTopRight);
        PlaneIntersection(ref this._planes.Near, ref this._planes.Bottom, ref this._planes.Left, out corners.NearBottomLeft);
        PlaneIntersection(ref this._planes.Near, ref this._planes.Bottom, ref this._planes.Right, out corners.NearBottomRight);
        PlaneIntersection(ref this._planes.Far, ref this._planes.Top, ref this._planes.Left, out corners.FarTopLeft);
        PlaneIntersection(ref this._planes.Far, ref this._planes.Top, ref this._planes.Right, out corners.FarTopRight);
        PlaneIntersection(ref this._planes.Far, ref this._planes.Bottom, ref this._planes.Left, out corners.FarBottomLeft);
        PlaneIntersection(ref this._planes.Far, ref this._planes.Bottom, ref this._planes.Right, out corners.FarBottomRight);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PlaneIntersection(ref Plane p1, ref Plane p2, ref Plane p3, out Vector3 intersection)
    {
        // Formula: http://geomalgorithms.com/a05-_intersect-1.html
        // The formula assumes that there is only a single intersection point.
        // Because of the way the frustum planes are constructed, this should be guaranteed.
        intersection =
            (-(p1.D * Vector3.Cross(p2.Normal, p3.Normal))
            - p2.D * Vector3.Cross(p3.Normal, p1.Normal)
            - p3.D * Vector3.Cross(p1.Normal, p2.Normal))
            / Vector3.Dot(p1.Normal, Vector3.Cross(p2.Normal, p3.Normal));
    }
}
