using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Imago.Rendering.Forward;
using Imago.Rendering.Materials;
using Imago.Rendering.Meshes;
using Imago.SceneGraph;
using Veldrid.Utilities;

namespace Imago.Rendering;

public struct OffsetVertexData // It's 16 bytes only!
{
    public uint TransformDataOffset { get; set; } // x
    public uint InstanceDataOffset { get; set; } // y
    public uint BoneDataOffset { get; set; } // z
    public uint PickingId { get; set; } // w
}

/// <summary>
/// Determines the mode the object will use to cast shadows.
/// </summary>
public enum ShadowCasting
{
    /// <summary>
    /// The object will cast shadows. This is the default mode.
    /// </summary>
    CastShadows,

    /// <summary>
    /// The object will not cast shadows.
    /// </summary>
    NoShadows,

    /// <summary>
    /// The object will only cast shadows and the object itself will not be visible.
    /// </summary>
    OnlyShadows,
}


/// <summary>
/// Represents a low-level renderable object in 3D space that can be rendered by the renderer.
/// This object contains the information about the mesh, material, and skeleton to use when rendering
/// as well as the transform and information for culling and batching.
/// </summary>
internal class Renderable : IDisposable
{
    // Contiguos layout
    [StructLayout(LayoutKind.Sequential)]
    private struct InstanceData
    {
        public InstanceData() { }
        public Vector4 AlbedoColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        public Vector4 TextureST = new Vector4(1.0f, 1.0f, 0.0f, 0.0f);
        public Vector4 HighlightColor = new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
    }

    private Matrix4x4 _transform = Matrix4x4.Identity;

    private InstanceData _instanceData = new InstanceData();

    /// <summary>
    /// Gets the center position of this renderable.
    /// </summary>
    public Vector3 CenterPosition { get; private set; }

    /// <summary>
    /// Gets the bounding box of this renderable.
    /// </summary>
    public BoundingBox BoundingBox { get; private set; }

    private DataBlock _transformDataBlock;
    private DataBlock _instanceDataBlock;

    /// <summary>
    /// Gets the batching hash of this renderable. This is used to
    /// determine if two renderables can be batched together as an early exit.
    /// </summary>
    public int BatchingHash { get; private set; }

    /// <summary>
    /// Gets the sort key of this renderable. This is used to sort renderables
    /// by material and mesh.
    /// </summary>
    public ulong SortKey { get; private set; }

    /// <summary>
    /// Gets the resource set for the transform data.
    /// </summary>
    public Veldrid.ResourceSet TransformResourceSet { get; private set; }

    /// <summary>
    /// Gets the resource set for the instance data.
    /// </summary>
    public Veldrid.ResourceSet InstanceResourceSet { get; private set; }

    /// <summary>
    /// Gets the resource set for the skeleton data.
    /// </summary>
    public Veldrid.ResourceSet? SkeletonResourceSet { get; private set; } = null;

    /// <summary>
    /// Gets the data for the "offset" vertex attribute.
    /// </summary>
    public OffsetVertexData OffsetVertexData { get; set; }

    private RenderQueues _renderQueueFlags = RenderQueues.None;

    /// <summary>
    /// Gets the render queues that this renderable belongs to.
    /// </summary>
    internal RenderQueues RenderQueues
    {
        get => this._renderQueueFlags;
        private set
        {
            if (this._renderQueueFlags != value)
            {
                var oldFlags = this._renderQueueFlags;
                this._renderQueueFlags = value;
                this._stage?.NotifyRenderableRenderQueueChanged(this, oldFlags, value);
            }
        }
    }

    private uint _pickingId = 0;

    /// <summary>
    /// Gets the picking ID of this renderable.
    /// </summary>
    public uint PickingId
    {
        get => this._pickingId;
        set
        {
            if (this._pickingId == value) return;

            this._pickingId = value;
            this.RecomputeOffsetVertexData();
            this.InvalidatePipeline();
        }
    }

    /// <summary>
    /// Gets the forward rendering pipeline for this renderable.
    /// </summary>
    public Veldrid.Pipeline? ForwardPipeline { get; private set; }

    /// <summary>
    /// Gets the shadow mapping pipeline for this renderable.
    /// </summary>
    public Veldrid.Pipeline? ShadowMapPipeline { get; private set; }

    /// <summary>
    /// Gets the picking pipeline for this renderable.
    /// </summary>
    public Veldrid.Pipeline? PickingPipeline { get; private set; }

    private bool _pipelineDirty = false;



    /// <summary>
    /// Initializes a new instance of the <see cref="Renderable"/> class.
    /// </summary>
    /// <param name="renderer">The renderer.</param>
    public Renderable(Renderer renderer)
    {
        var transformDataBlock = renderer.RequestTransformDataBlock();
        var instanceDataBlock = renderer.RequestInstanceDataBlock(Marshal.SizeOf<InstanceData>());

        this._transformDataBlock = transformDataBlock;
        this.TransformResourceSet = this._transformDataBlock.Buffer.ResourceSet;

        this._instanceDataBlock = instanceDataBlock;
        this.InstanceResourceSet = this._instanceDataBlock.Buffer.ResourceSet;

        this.RecomputeOffsetVertexData();

        this._instanceDataBlock.Write(ref this._instanceData);
    }

    private Stage? _stage = null;

    /// <summary>
    /// Gets or sets the stage in which this renderable is.
    /// </summary>
    public Stage? Stage
    {
        get => this._stage;
        set
        {
            if (this._stage == value) return;

            if (this._stage != null)
            {
                this._stage.NotifyRenderablePipelineDirty(this);
                this._stage.NotifyRenderableRenderQueueChanged(this, this._renderQueueFlags, RenderQueues.None);
            }

            this._stage = value;

            if (this._stage != null)
            {
                this._stage.NotifyRenderablePipelineDirty(this);
                this._stage.NotifyRenderableRenderQueueChanged(this, RenderQueues.None, this._renderQueueFlags);
            }
        }
    }

    private Mesh? _mesh;

    /// <summary>
    /// Gets or sets the mesh.
    /// </summary>
    public Mesh? Mesh
    {
        get => this._mesh;
        set
        {
            if (this._mesh == value) return;
            this._mesh = value;

            this.RecomputeBoundingBox();
            this.InvalidatePipeline();
        }
    }


    private Material? _material;

    /// <summary>
    /// Gets or sets the material.
    /// </summary>
    public Material? Material
    {
        get => this._material;
        set
        {
            if (this._material == value) return;
            this._material = value;

            this.InvalidatePipeline();
        }
    }

    private Skeleton? _skeleton;

    /// <summary>
    /// Gets or sets the skeleton.
    /// </summary>
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
            this.InvalidatePipeline();
        }
    }

    /// <summary>
    /// Gets or sets the transform.
    /// </summary>
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

    private bool _visible = true;

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="Renderable"/> is visible.
    /// </summary>
    public bool Visible
    {
        get => this._visible;
        set
        {
            if (this._visible == value) return;
            this._visible = value;
            this.InvalidatePipeline();
        }
    }

    private bool _transparent = false;

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="Renderable"/> uses transparency.
    /// </summary>
    public bool Transparent
    {
        get => this._transparent;
        set
        {
            if (this._transparent == value) return;
            this._transparent = value;
            this.InvalidatePipeline();
        }
    }

    private ShadowCasting _shadowCastingMode = ShadowCasting.CastShadows;

    /// <summary>
    /// Gets or sets the shadow casting mode.
    /// </summary>
    public ShadowCasting ShadowCastingMode
    {
        get => this._shadowCastingMode;
        set
        {
            if (this._shadowCastingMode == value) return;
            this._shadowCastingMode = value;
            this.InvalidatePipeline();
        }
    }

    /// <summary>
    /// Invalidates the pipeline.
    /// </summary>
    public void InvalidatePipeline()
    {
        if (this._pipelineDirty) return;

        this._pipelineDirty = true;
        this._stage?.NotifyRenderablePipelineDirty(this);
    }

    /// <summary>
    /// Gets or sets the albedo color.
    /// </summary>
    public Vector4 AlbedoColor
    {
        get => this._instanceData.AlbedoColor;
        set => this.SetInstanceData(ref this._instanceData.AlbedoColor, value);
    }

    /// <summary>
    /// Gets or sets the texture ST.
    /// </summary>
    public Vector4 TextureST
    {
        get => this._instanceData.TextureST;
        set => this.SetInstanceData(ref this._instanceData.TextureST, value);
    }

    /// <summary>
    /// Gets or sets the highlight color.
    /// </summary>
    public Vector4 HighlightColor
    {
        get => this._instanceData.HighlightColor;
        set => this.SetInstanceData(ref this._instanceData.HighlightColor, value);
    }

    /// <summary>
    /// Gets or sets the opacity.
    /// </summary>
    public float Opacity
    {
        get => this._instanceData.AlbedoColor.W;
        set
        {
            if (this.SetInstanceData(ref this._instanceData.AlbedoColor.W, value))
            {
                this.Transparent = value < 1.0f;
            }
        }
    }

    protected bool SetInstanceData<T>(ref T backingField, T value) where T : unmanaged
    {
        if (backingField.Equals(value)) return false;
        backingField = value;
        this._instanceDataBlock.Write(ref this._instanceData);
        return true;
    }

    /// <summary>
    /// Updates the renderable.
    /// </summary>
    /// <param name="renderer">The renderer.</param>
    public void Update(Renderer renderer)
    {
        if (this._pipelineDirty)
        {
            this._pipelineDirty = false;
            this.RecomputeSortKey();
            this.RecomputeRenderQueue();
            this.RecomputePipeline(renderer);
        }
    }

    protected void RecomputeSortKey()
    {
        if (this.Material == null || this.Mesh == null) return;

        ulong materialHash         = (ulong) (this.Material.GetHashCode() & 0xFFF);
        ulong meshHash             = (ulong) (this.Mesh.GetHashCode() & 0xFFF);
        ulong transformBufferHash  = (ulong) (this.TransformResourceSet.GetHashCode() & 0xFFF);
        ulong instanceBufferHash   = (ulong) (this.InstanceResourceSet.GetHashCode() & 0xFFF);
        ulong skekeletonBufferHash = (ulong) (this.SkeletonResourceSet?.GetHashCode() ?? 0 & 0xFF);

        // The sort key is a 64-bit number that is used to sort renderables.
        this.SortKey = (materialHash << 56)  // 8 bits (max: 255)
                | (transformBufferHash << 48)  // 8 bits (max: 255)
                | (instanceBufferHash << 40)  // 8 bits (max: 255)
                | (skekeletonBufferHash << 36)  // 4 bits (max: 15)
                | (meshHash << 24); // 12 bits (max: 4095)

        // This key is used to fast check if two instances can be batched together. (They must have the same hash)
        // It could happen that the hash is not unique, but it is extremely unlikely that two instances that are
        // contiguous after sorting the render queue by the sort key, end up having the same hash.
        this.BatchingHash = HashCode.Combine(
            this.Mesh.Id,
            this.Material.Id,
            instanceBufferHash,
            skekeletonBufferHash,
            transformBufferHash
        );
    }


    /// <summary>
    /// Checks if this renderable can be batched with another renderable.
    /// </summary>
    /// <param name="other">The other renderable.</param>
    /// <returns>True if the renderables can be batched together, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanBeBatchedWith(Renderable other)
    {
        // First, fast check using the hash key. If the keys are different, we can early out.
        // If the keys are equal, we need to check the actual data.
        return this.BatchingHash == other.BatchingHash
            && this.Mesh == other.Mesh
            && this.Material == other.Material
            && this.SkeletonResourceSet == other.SkeletonResourceSet
            && this.InstanceResourceSet == other.InstanceResourceSet
            && this.TransformResourceSet == other.TransformResourceSet;
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
        return this.SortKey | (cameraDistance & 0xFFFFFF); // 24 bits
    }

    private void RecomputeRenderQueue()
    {
        if (this.Material is null || this.Mesh is null)
        {
            this.RenderQueues = RenderQueues.None;
            return;
        }

        RenderQueues flags = RenderQueues.None;

        if (this.Visible)
        {
            if (this.ShadowCastingMode != ShadowCasting.OnlyShadows)
            {
                flags |= this.Material.Transparent || this._transparent ? RenderQueues.Transparent : RenderQueues.Opaque;
            }

            if (this.ShadowCastingMode != ShadowCasting.NoShadows)
            {
                flags |= RenderQueues.ShadowCaster;
            }
        }

        if (this.PickingId != 0)
        {
            flags |= RenderQueues.Picking;
        }

        this.RenderQueues = flags;
    }

    private void RecomputePipeline(Renderer renderer)
    {
        if (this._material == null || this._mesh == null || this.RenderQueues == RenderQueues.None)
        {
            this.ForwardPipeline = null;
            this.ShadowMapPipeline = null;
            this.PickingPipeline = null;
            return;
        }

        this.ForwardPipeline = this.GetForwardPipeline(this._material, renderer.Settings);
        this.ShadowMapPipeline = this.GetShadowmapPipeline(this._material);
        this.PickingPipeline = this.GetPickingPipeline(this._material);
    }

    private Veldrid.Pipeline? GetForwardPipeline(Material material, RenderSettings settings)
    {
        bool isVisible = (this.RenderQueues & RenderQueues.OpaqueOrTransparent) != 0;
        if (!isVisible) return null;

        RenderFlags flags = RenderFlags.None;
        if (settings.ForceWireframe) flags |= RenderFlags.Wireframe;
        if (this.RenderQueues.HasFlag(RenderQueues.Transparent)) flags |= RenderFlags.Transparent;
        if (settings.EnableFog) flags |= RenderFlags.Fog;
        if (settings.EnablePixelPerfectShadows) flags |= RenderFlags.PixelPerfactShadows;

        return material.ForwardShader.GetPipeline(this._mesh!.VertexFormat, material.RenderFlags | flags);
    }

    private Veldrid.Pipeline? GetShadowmapPipeline(Material material)
    {
        bool isShadowcaster = (this.RenderQueues & RenderQueues.ShadowCaster) != 0;
        if (!isShadowcaster) return null;

        RenderFlags shadowSupportFlags = RenderFlags.AlphaTest;
        RenderFlags flags = material.RenderFlags & shadowSupportFlags;
        return material.ShadowMapShader.GetPipeline(this._mesh!.VertexFormat, flags);
    }

    private Veldrid.Pipeline? GetPickingPipeline(Material material)
    {
        bool isPicking = (this.RenderQueues & RenderQueues.Picking) != 0;
        if (!isPicking) return null;

        return material.PickingShader.GetPipeline(this._mesh!.VertexFormat, material.RenderFlags);
    }

    /// <summary>
    /// Disposes the renderable.
    /// </summary>
    public void Dispose()
    {
        this.RenderQueues = RenderQueues.None;
        this._pipelineDirty = false;
        this._transformDataBlock.Dispose();
        this._instanceDataBlock.Dispose();
    }
}
