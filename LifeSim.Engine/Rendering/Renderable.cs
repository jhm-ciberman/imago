using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering;

public class Renderable : IDisposable
{

    private static readonly SwapPopList<Renderable> _renderables = new SwapPopList<Renderable>();
    public delegate void RenderQueuesChangedHandler(Renderable renderable, RenderQueues oldFlags, RenderQueues newFlags);
    public delegate void PipelineDirtyHandler(Renderable renderable);
    public static event RenderQueuesChangedHandler? RenderQueuesChanged;
    public static event PipelineDirtyHandler? PipelineDirty;

    private static bool _forceWireframe = false;
    public static bool ForceWireframe
    {
        get => _forceWireframe;
        set
        {
            if (_forceWireframe == value) return;
            _forceWireframe = value;
            foreach (var renderable in _renderables)
            {
                renderable.NotifyPipelineDirty();
            }
        }
    }

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
    private RenderQueues _renderQueueFlags = RenderQueues.All;
    public RenderQueues RenderQueueFlags
    {
        get => this._renderQueueFlags;
        private set
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
            if (this._pickingId == value) return;

            this._pickingId = value;
            this.RecomputeOffsetVertexData();
        }
    }

    private Mesh? _mesh;
    private Skeleton? _skeleton;
    private Material? _material;
    private bool _visible = true;
    private ShadowCasting _shadowCastingMode = ShadowCasting.CastShadows;

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

        _renderables.Add(this);
    }

    public Mesh? Mesh
    {
        get => this._mesh;
        set
        {
            if (this._mesh == value) return;
            this._mesh = value;

            this.RenderQueueFlagsChanged();
            this.RecomputeBoundingBox();
            this.NotifyPipelineDirty();
        }
    }

    public Material? Material
    {
        get => this._material;
        set
        {
            if (this._material == value) return;
            this._material = value;

            this.RenderQueueFlagsChanged();
            this.RecomputeOffsetVertexData();
            this.NotifyPipelineDirty();
        }
    }

    public Skeleton? Skeleton
    {
        get => this._skeleton;
        set
        {
            if (this._skeleton == value) return;

            this._skeleton = value;
            this.SkeletonResourceSet = this._skeleton?.ResourceSet;
            this.UpdateSkeletonTransform();
            this.RecomputeOffsetVertexData();
            this.NotifyPipelineDirty();
        }
    }

    public Matrix4x4 Transform
    {
        get => this._transform;
        set
        {
            this._transform = value;
            this._transformDataBlock.Write(ref value);

            this.RecomputeBoundingBox();
            this.UpdateSkeletonTransform();
        }
    }

    public bool Visible
    {
        get => this._visible;
        set
        {
            if (this._visible == value) return;
            this._visible = value;
            this.RenderQueueFlagsChanged();
        }
    }


    public ShadowCasting ShadowCastingMode
    {
        get => this._shadowCastingMode;
        set
        {
            if (this._shadowCastingMode == value) return;
            this._shadowCastingMode = value;
            this.RenderQueueFlagsChanged();
        }
    }

    protected void NotifyPipelineDirty()
    {
        if (this._pipelineDirty) return;
        this._pipelineDirty = true;
        PipelineDirty?.Invoke(this);
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

            if (this._material != null && this._mesh != null && this.RenderQueueFlags != RenderQueues.None)
            {
                // Update the forward pipeline
                if (this.RenderQueueFlags.HasFlag(RenderQueues.Opaque))
                {
                    this.ForwardPipeline = this._material.GetForwardPipeline(renderer, this._mesh.VertexFormat, _forceWireframe);
                }
                else if (this.RenderQueueFlags.HasFlag(RenderQueues.Transparent))
                {
                    this.ForwardPipeline = this._material.GetForwardPipeline(renderer, this._mesh.VertexFormat, _forceWireframe);
                }
                else
                {
                    this.ForwardPipeline = null;
                }

                // Update the shadow map pipeline
                if (this.RenderQueueFlags.HasFlag(RenderQueues.ShadowCaster))
                {
                    this.ShadowMapPipeline = this._material.GetShadowmapPipeline(renderer, this._mesh.VertexFormat);
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
        if (this._material == null || this._mesh == null) return;

        ulong materialHash        = (ulong) (this._material.Id & 0xFFF);
        ulong meshHash            = (ulong) (this._mesh.Id & 0xFFF);
        ulong transformBufferHash = (ulong) (this._transformDataBlock.Buffer.Id & 0xFF);
        ulong instanceBufferHash  = (ulong) (this._instanceDataBlock.Buffer.Id & 0xFF);
        ulong skekeletonBufferHash = (this._skeleton != null) ? (ulong)(this._skeleton.BufferId & 0xF) : 0;

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
            this._mesh.Id,
            this._material.Id,
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
            && this._mesh == other._mesh
            && this._material == other._material
            && this._skeleton?.Buffer == other._skeleton?.Buffer
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
            BoneDataOffset = this._skeleton?.BoneDataOffset ?? 0,
            PickingId = this.PickingId, // this id is used for picking
        };
    }

    private void RecomputeBoundingBox()
    {
        if (this.Mesh != null)
        {
            this.BoundingBox = BoundingBox.Transform(this.Mesh.BoundingBox, this.Transform);
            this.CenterPosition = this.BoundingBox.GetCenter();
        }
    }

    private void UpdateSkeletonTransform()
    {
        if (this._skeleton != null)
        {
            _ = Matrix4x4.Invert(this._transform, out Matrix4x4 inverseRootTransform);
            this._skeleton.InverseRootTransform = inverseRootTransform;
        }
    }

    internal ulong GetSortKey(Vector3 cameraPosition)
    {
        float dist = Vector3.DistanceSquared(this.CenterPosition, cameraPosition);
        uint cameraDistance = Math.Min(uint.MaxValue, (uint) (dist * 1000f));
        return this._cachedSortKey | (cameraDistance & 0xFFFFFF); // 24 bits
    }

    private void RenderQueueFlagsChanged()
    {
        RenderQueues flags = RenderQueues.None;

        if (this.Material is null || this.Mesh is null)
        {
            this.RenderQueueFlags = flags;
            return;
        }

        if (this.Visible)
        {
            flags |= RenderQueues.Opaque;

            if (this.ShadowCastingMode == ShadowCasting.CastShadows)
            {
                flags |= RenderQueues.ShadowCaster;
            }
        }
        else if (this.ShadowCastingMode == ShadowCasting.OnlyShadows)
        {
            flags |= RenderQueues.ShadowCaster;
        }

        this.RenderQueueFlags = flags;
    }

    public void Dispose()
    {
        this._instanceDataBlock.FreeBlock();
        this._transformDataBlock.FreeBlock();
        _renderables.Remove(this);
    }
}