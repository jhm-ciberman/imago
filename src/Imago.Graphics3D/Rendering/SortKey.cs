using System;

namespace Imago.Graphics3D.Rendering;

/// <summary>
/// The 64-bit number used to order a renderable in a render queue.
/// </summary>
internal readonly struct SortKey : IComparable<SortKey>
{
    private const int DistanceBits = 24;
    private const int MeshBits = 12;
    private const int SkeletonBits = 4;
    private const int InstanceBits = 8;
    private const int TransformBits = 8;
    private const int MaterialBits = 8;

    private const int MeshShift = DistanceBits;
    private const int SkeletonShift = MeshShift + MeshBits;
    private const int InstanceShift = SkeletonShift + SkeletonBits;
    private const int TransformShift = InstanceShift + InstanceBits;
    private const int MaterialShift = TransformShift + TransformBits;

    private const int DistanceFirstShift = 32;

    private const float DistanceLastScale = 16f;
    private const float DistanceFirstScale = 1000f;

    private readonly ulong _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="SortKey"/> struct from the render state.
    /// </summary>
    /// <param name="materialId">The id of the material.</param>
    /// <param name="transformHash">The hash of the transform buffer.</param>
    /// <param name="instanceHash">The hash of the instance buffer.</param>
    /// <param name="skeletonHash">The hash of the skeleton buffer, or 0 if there is no skeleton.</param>
    /// <param name="meshId">The id of the mesh.</param>
    public SortKey(int materialId, int transformHash, int instanceHash, int skeletonHash, int meshId)
    {
        this._value = Pack((uint)materialId, MaterialBits, MaterialShift)
            | Pack((uint)transformHash, TransformBits, TransformShift)
            | Pack((uint)instanceHash, InstanceBits, InstanceShift)
            | Pack((uint)skeletonHash, SkeletonBits, SkeletonShift)
            | Pack((uint)meshId, MeshBits, MeshShift);
    }

    private SortKey(ulong value)
    {
        this._value = value;
    }

    /// <summary>
    /// Returns a key that orders by render state first, and by the given distance after that.
    /// </summary>
    /// <param name="distanceSquared">The squared distance to the camera.</param>
    /// <returns>The new key.</returns>
    public SortKey WithDistanceLast(float distanceSquared)
    {
        ulong maxDistance = (1UL << DistanceBits) - 1;
        ulong distance = ulong.Min(ToFixedPoint(distanceSquared, DistanceLastScale), maxDistance);

        return new SortKey(this._value | distance);
    }

    /// <summary>
    /// Returns a key that orders by the given distance first, and by render state after that.
    /// </summary>
    /// <param name="distanceSquared">The squared distance to the camera.</param>
    /// <returns>The new key.</returns>
    public SortKey WithDistanceFirst(float distanceSquared)
    {
        ulong distance = ToFixedPoint(distanceSquared, DistanceFirstScale);

        return new SortKey(distance << DistanceFirstShift | this._value >> DistanceFirstShift);
    }

    /// <inheritdoc/>
    public int CompareTo(SortKey other)
    {
        return this._value.CompareTo(other._value);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return this._value.ToString("x16");
    }

    private static ulong ToFixedPoint(float distanceSquared, float scale)
    {
        return uint.CreateSaturating(distanceSquared * scale);
    }

    private static ulong Pack(ulong value, int bits, int shift)
    {
        return (value & ((1UL << bits) - 1)) << shift;
    }
}
