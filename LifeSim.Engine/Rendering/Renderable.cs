using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public class Renderable
    {
        private static uint _count;

        public int RenderListIndex { get; set; }
        private Matrix4x4 _transform = Matrix4x4.Identity;
        private Vector3 _centerPosition;
        public BoundingBox BoundingBox;
        private ulong _cachedSortKey;
        public int BatchingHashKey { get; private set; }

        internal Veldrid.ResourceSet TransformResourceSet { get; private set; }
        internal Veldrid.ResourceSet? InstanceResourceSet { get; private set; } = null;
        internal Veldrid.ResourceSet? SkeletonResourceSet { get; private set; }

        public OffsetVertexData OffsetVertexData { get; set; }

        private DataBlock _transformDataBlock;

        public Mesh? Mesh { get; private set; } = null;

        public bool Visible { get; set; } = true;
        public uint Id { get; }

        public Skeleton? Skeleton { get; private set; }
        public Material? Material { get; private set; }
        private DataBlock _instanceDataBlock;
        private readonly SceneStorage _storage;

        public Renderable(SceneStorage storage)
        {
            this._storage = storage;
            this.Id = ++_count;
            this._transformDataBlock = storage.RequestTransformDataBlock();
            this.TransformResourceSet = this._transformDataBlock.Buffer.ResourceSet;
        }

        public Renderable(SceneStorage storage, Mesh mesh) : this(storage)
        {
            this.SetMesh(mesh);
        }

        public void SetMesh(Mesh mesh)
        {
            this.Mesh = mesh;
            this._RecomputeSortKey();
            this._RecomputeBoundingBox();
        }

        public void SetMaterial(Material material)
        {
            this.Material = material;
            if (this._instanceDataBlock.BlockSize != material.Definition.InstanceDataBlockSize)
            {
                this._instanceDataBlock.FreeBlock();
                this._instanceDataBlock = this._storage.RequestInstanceDataBlock(material.Definition);
            }
            this.InstanceResourceSet = this._instanceDataBlock.Buffer.ResourceSet;
            var span = material.Definition.GetDefaultInstanceData();
            this._instanceDataBlock.WriteSpan(span);
            this._RecomputeOffsetVertexData();
            this._RecomputeSortKey();
        }

        public void SetSkeleton(Skeleton skeleton)
        {
            if (this.Skeleton == skeleton) return;
            this.Skeleton = skeleton;
            this.SkeletonResourceSet = skeleton.ResourceSet;
            this._RecomputeOffsetVertexData();
            this._RecomputeSortKey();
        }

        public void SetTransform(ref Matrix4x4 transform)
        {
            this._transform = transform;
            this._transformDataBlock.Write(ref transform);

            if (this.Mesh != null)
            {
                this._RecomputeBoundingBox();
            }

            if (this.Skeleton != null)
            {
                this.Skeleton.RootTransform = transform;
            }
        }

        public void SetInstanceData<T>(string name, T data) where T : unmanaged
        {
            if (this.Material == null) throw new Exception("No material set");
            var offset = this.Material.Definition.GetInstanceUniformDataOffset(name);
            this._instanceDataBlock.Write(offset, ref data);
        }

        public void Free()
        {
            this._instanceDataBlock.FreeBlock();
            this._transformDataBlock.FreeBlock();
        }

        protected void _RecomputeSortKey()
        {
            if (this.Material == null || this.Mesh == null) return;

            ulong materialHash        = (ulong) (this.Material.Id & 0xFFF);
            ulong meshHash            = (ulong) (this.Mesh.Id & 0xFFF);
            ulong transformBufferHash = (ulong) (this._transformDataBlock.Buffer.Id & 0xFF);
            //ulong instanceBufferHash  = (ulong) (this._instanceDataBlock.Buffer.Id & 0xFF);
            ulong skekeletonBufferHash = (this.Skeleton != null) ? (ulong)(this.Skeleton.BufferId & 0xF) : 0;

            // The sort key is a 64-bit number that is used to sort renderables.
            ulong key = (materialHash   << 56) // 8 bits
                | (transformBufferHash  << 48) // 8 bits
                | (transformBufferHash  << 40) // 8 bits
                | (skekeletonBufferHash << 36) // 4 bits
                | (meshHash             << 24); // 12 bits

            this._cachedSortKey = key;

            // This key is used to fast check if two instances can be batched together. (They must have the same hash)
            this.BatchingHashKey = HashCode.Combine(
                this.Mesh.Id,
                this.Material.Id,
                this._instanceDataBlock.Buffer.Id,
                skekeletonBufferHash,
                this._transformDataBlock.Buffer.Id
            );
        }

        private void _RecomputeOffsetVertexData()
        {
            if (!this._instanceDataBlock.IsValid) return;

            this.OffsetVertexData = new OffsetVertexData(
                this._transformDataBlock.BlockIndex,
                this._instanceDataBlock.BlockIndex,
                this.Skeleton?.BoneDataOffset ?? 0,
                this.Id // this id is used for picking
            );
        }

        private void _RecomputeBoundingBox()
        {
            this.BoundingBox = BoundingBox.Transform(this.Mesh!.AABB, this._transform);
            this._centerPosition = this.BoundingBox.GetCenter();
        }

        internal ulong GetSortKey(Vector3 cameraPosition)
        {
            float dist = Vector3.DistanceSquared(this._centerPosition, cameraPosition);
            uint cameraDistance = Math.Min(uint.MaxValue, (uint) (dist * 1000f));
            return this._cachedSortKey | (cameraDistance & 0xFFFFFF); // 24 bits
        }
    }
}