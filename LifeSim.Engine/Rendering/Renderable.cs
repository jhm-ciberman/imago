using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using LifeSim.Engine.Rendering;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering;

public class Renderable : IDisposable
{
    public delegate void RenderQueuesChangedHandler(Renderable renderable, RenderQueues oldFlags, RenderQueues newFlags);
    public delegate void PipelineDirtyHandler(Renderable renderable);
    public static event RenderQueuesChangedHandler? RenderQueuesChanged;
    public static event PipelineDirtyHandler? PipelineDirty;

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

    private RenderQueues _renderQueueFlags = RenderQueues.All;
    public RenderQueues RenderQueueFlags
    {
        get => this._renderQueueFlags;
        set
        {
            if (this._renderQueueFlags != value)
            {
                var oldFlags = this._renderQueueFlags;
                this._renderQueueFlags = value;
                RenderQueuesChanged?.Invoke(this, oldFlags, value);
            }
        }
    }

    private uint _pickingId = 0;
    public uint PickingId
    {
        get => this._pickingId;
        set
        {
            if (this._pickingId != value)
            {
                this._pickingId = value;
                this.RecomputeOffsetVertexData();
            }
        }
    }

    public Skeleton? Skeleton { get; private set; }
    public Material? Material { get; private set; }

    public Veldrid.Pipeline? ForwardPipeline { get; private set; }

    public Veldrid.Pipeline? ShadowMapPipeline { get; private set; }

    private bool _pipelineDirty = false;

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
        if (this.Mesh == mesh) return;
        this.Mesh = mesh;

        this.RecomputeBoundingBox();
        this.NotifyPipelineDirty();
    }

    public void SetMaterial(Material material)
    {
        if (this.Material == material) return;
        this.Material = material;

        this.RecomputeOffsetVertexData();
        this.NotifyPipelineDirty();
    }

    public void SetSkeleton(Skeleton skeleton)
    {
        if (this.Skeleton == skeleton) return;

        this.Skeleton = skeleton;
        _ = Matrix4x4.Invert(this._transform, out Matrix4x4 inverseRootTransform);
        this.Skeleton.InverseRootTransform = inverseRootTransform;
        this.SkeletonResourceSet = skeleton.ResourceSet;
        this.RecomputeOffsetVertexData();
        this.NotifyPipelineDirty();
    }

    protected void NotifyPipelineDirty()
    {
        if (this._pipelineDirty) return;
        this._pipelineDirty = true;
        PipelineDirty?.Invoke(this);
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
            _ = Matrix4x4.Invert(this._transform, out Matrix4x4 inverseRootTransform);
            this.Skeleton.InverseRootTransform = inverseRootTransform;
        }
    }

    public void SetInstanceData<T>(T data) where T : unmanaged
    {
        this._instanceDataBlock.Write(ref data);
    }

    public void Update(Renderer renderer)
    {
        if (this._pipelineDirty)
        {
            this._pipelineDirty = false;

            this.RecomputeSortKey();

            if (this.Material != null && this.Mesh != null && this.RenderQueueFlags != RenderQueues.None)
            {
                // Update the forward pipeline
                if (this.RenderQueueFlags.HasFlag(RenderQueues.Opaque))
                {
                    this.ForwardPipeline = this.Material.GetForwardPipeline(renderer, this.Mesh.VertexFormat);
                }
                else if (this.RenderQueueFlags.HasFlag(RenderQueues.Transparent))
                {
                    this.ForwardPipeline = this.Material.GetForwardPipeline(renderer, this.Mesh.VertexFormat);
                }
                else
                {
                    this.ForwardPipeline = null;
                }

                // Update the shadow map pipeline
                if (this.RenderQueueFlags.HasFlag(RenderQueues.ShadowCaster))
                {
                    this.ShadowMapPipeline = this.Material.GetShadowmapPipeline(renderer, this.Mesh.VertexFormat);
                }
                else
                {
                    this.ShadowMapPipeline = null;
                }
            }
            else
            {
                this.ForwardPipeline = null;
                this.ShadowMapPipeline = null;
            }
        }
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
        // First, fast check using the hash key. If the keys are different, we can early out.
        // If the keys are equal, we need to check the actual data.
        return this._batchingHashKey == other._batchingHashKey
            && this.Mesh == other.Mesh
            && this.Material == other.Material
            && this.Skeleton?.Buffer == other.Skeleton?.Buffer
            && this._instanceDataBlock.Buffer == other._instanceDataBlock.Buffer
            && this._transformDataBlock.Buffer == other._transformDataBlock.Buffer;
    }

    private void RecomputeOffsetVertexData()
    {
        if (!this._instanceDataBlock.IsValid) return;

        this.OffsetVertexData = new OffsetVertexData
        {
            TransformDataOffset = this._transformDataBlock.BlockIndex,
            InstanceDataOffset = this._instanceDataBlock.BlockIndex,
            BoneDataOffset = this.Skeleton?.BoneDataOffset ?? 0,
            PickingId = this.PickingId, // this id is used for picking
        };
    }

    private void RecomputeBoundingBox()
    {
        this.BoundingBox = BoundingBox.Transform(this.Mesh!.BoundingBox, this._transform);
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