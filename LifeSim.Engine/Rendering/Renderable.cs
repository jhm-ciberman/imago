using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using LifeSim.Engine.Resources;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering;

public class Renderable : IDisposable
{
    public delegate void RenderQueueFlagsChangedHandler(Renderable renderable, RenderQueueFlags oldFlags, RenderQueueFlags newFlags);

    public static event RenderQueueFlagsChangedHandler? OnRenderQueueFlagsChanged;

    private static uint _count;
    private Matrix4x4 _transform = Matrix4x4.Identity;
    public Vector3 CenterPosition { get; private set; }
    public BoundingBox BoundingBox { get; private set; }
    private ulong _cachedSortKey;
    private int _batchingHashKey; // This key is used as an early exit for the batching system.
    internal Veldrid.ResourceSet TransformResourceSet { get; private set; }
    internal Veldrid.ResourceSet? InstanceResourceSet { get; private set; } = null;
    internal Veldrid.ResourceSet? SkeletonResourceSet { get; private set; } = null;

    public OffsetVertexData OffsetVertexData { get; set; }

    private DataBlock _transformDataBlock;

    public Mesh? Mesh { get; private set; } = null;

    private RenderQueueFlags _renderQueueFlags = RenderQueueFlags.All;
    public RenderQueueFlags RenderQueueFlags
    {
        get => this._renderQueueFlags;
        set
        {
            if (this._renderQueueFlags != value)
            {
                var oldFlags = this._renderQueueFlags;
                this._renderQueueFlags = value;
                OnRenderQueueFlagsChanged?.Invoke(this, oldFlags, value);
            }
        }
    }
    public uint PickingId { get; }

    public Skeleton? Skeleton { get; private set; }
    public Material? Material { get; private set; }
    private DataBlock _instanceDataBlock;

    public Renderable(SceneStorage storage, int instanceDataSize)
    {
        this.PickingId = ++_count;
        this._transformDataBlock = storage.RequestTransformDataBlock();
        this.TransformResourceSet = this._transformDataBlock.Buffer.ResourceSet;

        this._instanceDataBlock = storage.RequestInstanceDataBlock(instanceDataSize);
        this.InstanceResourceSet = this._instanceDataBlock.Buffer.ResourceSet;
    }

    public void SetMesh(Mesh mesh)
    {
        this.Mesh = mesh;
        this.RecomputeSortKey();
        this.RecomputeBoundingBox();
    }

    public void SetMaterial(Material material)
    {
        this.Material = material;

        this.RecomputeOffsetVertexData();
        this.RecomputeSortKey();
    }

    public void SetSkeleton(Skeleton skeleton)
    {
        this.Skeleton = skeleton;
        this.SkeletonResourceSet = skeleton.ResourceSet;
        this.RecomputeOffsetVertexData();
        this.RecomputeSortKey();
    }

    public void SetTransform(ref Matrix4x4 transform)
    {
        this._transform = transform;
        this._transformDataBlock.Write(ref transform);

        if (this.Mesh != null)
        {
            this.RecomputeBoundingBox();
        }

        if (this.Skeleton != null)
        {
            this.Skeleton.RootTransform = transform;
        }
    }

    public void SetInstanceData<T>(T data) where T : unmanaged
    {
        this._instanceDataBlock.Write(ref data);
    }

    protected void RecomputeSortKey()
    {
        if (this.Material == null || this.Mesh == null) return;

        ulong materialHash        = (ulong) (this.Material.Id & 0xFFF);
        ulong meshHash            = (ulong) (this.Mesh.Id & 0xFFF);
        ulong transformBufferHash = (ulong) (this._transformDataBlock.Buffer.Id & 0xFF);
        ulong instanceBufferHash  = (ulong) (this._instanceDataBlock.Buffer.Id & 0xFF);
        ulong skekeletonBufferHash = (this.Skeleton != null) ? (ulong)(this.Skeleton.BufferId & 0xF) : 0;

        // The sort key is a 64-bit number that is used to sort renderables.
        ulong key = (materialHash   << 56) // 8 bits (max: 255)
                | (transformBufferHash  << 48) // 8 bits (max: 255)
                | (instanceBufferHash   << 40) // 8 bits (max: 255)
                | (skekeletonBufferHash << 36) // 4 bits (max: 15)
                | (meshHash             << 24); // 12 bits (max: 4095)

        this._cachedSortKey = key;

        // This key is used to fast check if two instances can be batched together. (They must have the same hash)
        // It could happen that the hash is not unique, but it is extremely unlikely that two instances that are 
        // contiguous after sorting the render queue by the sort key, end up having the same hash.
        this._batchingHashKey = HashCode.Combine(
            this.Mesh.Id,
            this.Material.Id,
            instanceBufferHash,
            skekeletonBufferHash,
            transformBufferHash
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanBeBatchedWith(Renderable other)
    {
        if (this._batchingHashKey != other._batchingHashKey) return false;

        if (this.Mesh != other.Mesh) return false;
        if (this.Material != other.Material) return false;
        if (this.Skeleton?.BufferId != other.Skeleton?.BufferId) return false;
        if (this._instanceDataBlock.Buffer.Id != other._instanceDataBlock.Buffer.Id) return false;
        if (this._transformDataBlock.Buffer.Id != other._transformDataBlock.Buffer.Id) return false;

        return true;
    }

    private void RecomputeOffsetVertexData()
    {
        if (!this._instanceDataBlock.IsValid) return;

        this.OffsetVertexData = new OffsetVertexData
        {
            TransformDataOffset = this._transformDataBlock.BlockIndex,
            InstanceDataOffset = this._instanceDataBlock.BlockIndex,
            BoneDataOffset = this.Skeleton?.BoneDataOffset ?? 0,
            PickingId = this.PickingId // this id is used for picking
        };
    }

    private void RecomputeBoundingBox()
    {
        this.BoundingBox = BoundingBox.Transform(this.Mesh!.AABB, this._transform);
        this.CenterPosition = this.BoundingBox.GetCenter();
    }

    internal ulong GetSortKey(Vector3 cameraPosition)
    {
        float dist = Vector3.DistanceSquared(this.CenterPosition, cameraPosition);
        uint cameraDistance = Math.Min(uint.MaxValue, (uint) (dist * 1000f));
        return this._cachedSortKey | (cameraDistance & 0xFFFFFF); // 24 bits
    }

    public void Dispose()
    {
        this._instanceDataBlock.FreeBlock();
        this._transformDataBlock.FreeBlock();
    }
}