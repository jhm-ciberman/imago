using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid.Utilities;

namespace LifeSim.Rendering
{
    public class Renderable
    {
        public const int MAX_NUMBER_OF_BONES = 64;
        public int RenderListIndex { get; set; }
        private Matrix4x4 _transform;
        private Vector3 _centerPosition;
        private BoundingBox _aabb;
        private ulong _cachedSortKey;
        public int BatchingHashKey { get; private set; }

        internal Veldrid.ResourceSet TransformResourceSet { get; private set; }
        internal Veldrid.ResourceSet? MaterialResourceSet { get; private set; } = null;
        internal Veldrid.ResourceSet? InstanceResourceSet { get; private set; } = null;
        internal Veldrid.ResourceSet? SkeletonResourceSet { get; private set; }

        public OffsetVertexData OffsetVertexData { get; set; }

        private DataBlock _transformDataBlock;
        private DataBlock _skeletonDataBlock;

        public Mesh? Mesh { get; private set; } = null;

        public bool Visible { get; set; } = true;

        private uint _pickingID = 0;
        public uint PickingID
        { 
            get => this._pickingID; 
            set { this._pickingID = value; this._RecomputeOffsetVertexData(); }
        }
 
        public ISkeleton? Skeleton  { get; private set; }
        public Material? Material { get; private set; }
        private DataBlock _instanceDataBlock;
        private readonly SceneStorage _storage;

        public Renderable(SceneStorage storage)
        {
            this._storage = storage;
            
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
        }

        public void SetMaterial(Material material)
        {
            this.Material = material;
            if (this._instanceDataBlock.BlockSize != material.Definition.InstanceDataBlockSize) {
                this._instanceDataBlock.FreeBlock();
                this._instanceDataBlock = this._storage.RequestInstanceDataBlock(material.Definition);
            }
            this.MaterialResourceSet = material.GetMaterialResourceSet();
            this.InstanceResourceSet = this._instanceDataBlock.Buffer.ResourceSet;
            var span = material.Definition.GetDefaultInstanceData();
            this._instanceDataBlock.WriteSpan(span);
            this._RecomputeOffsetVertexData();
            this._RecomputeSortKey();
        }

        public void SetSkeleton(ISkeleton skeleton)
        {
            if (this.Skeleton == skeleton) return;
            if (! this._skeletonDataBlock.IsValid) {
                this._skeletonDataBlock = this._storage.RequestSkeletonDataBlock();
                this.SkeletonResourceSet = this._skeletonDataBlock.Buffer.ResourceSet;
                this._RecomputeOffsetVertexData();
                this._RecomputeSortKey();
            }
            this.Skeleton = skeleton;
        }

        public void Update()
        {
            if (this.Skeleton != null) {
                Matrix4x4.Invert(this._transform, out Matrix4x4 inverseMeshWorldMatrix);
                this.Skeleton.UpdateMatrices(ref inverseMeshWorldMatrix);
                this._skeletonDataBlock.WriteSpan<Matrix4x4>(this.Skeleton.BonesMatrices);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Cull(ref BoundingFrustum frustum)
        {
            return frustum.Contains(this._aabb) != ContainmentType.Disjoint;
        }

        public void SetTransform(ref Matrix4x4 transform)
        {
            this._transform = transform;
            this._transformDataBlock.Write(ref transform);
            if (this.Mesh != null) {
                this._RecomputeBoundingBox();
            }
        }

        public void SetPosition(Vector3 position)
        {
            var mat = Matrix4x4.CreateTranslation(position);
            this.SetTransform(ref mat);
        }


        public void SetInstanceData<T>(ref T data) where T : unmanaged
        {
            this._instanceDataBlock.Write(ref data);
        }

        public void SetInstanceData<T>(string name, T data) where T : unmanaged
        {
            if (this.Material == null) throw new Exception("No material set");
            var offset = this.Material.Definition.InstanceUniformData[name];
            this._instanceDataBlock.Write(offset, ref data);
        }

        public void Free()
        {
            this._instanceDataBlock.FreeBlock();
            this._transformDataBlock.FreeBlock();
            this._skeletonDataBlock.FreeBlock();
        }

        protected void _RecomputeSortKey()
        {
            if (this.Material == null || this.Mesh == null) return;

            ulong materialHash        = (ulong) (this.Material.Id & 0xFFF);
            ulong meshHash            = (ulong) (this.Mesh.Id & 0xFFF);
            ulong transformBufferHash = (ulong) (this._transformDataBlock.Buffer.Id & 0xFF);
            //ulong instanceBufferHash  = (ulong) (this._instanceDataBlock.Buffer.Id & 0xFF);
            ulong skekeletonBufferHash = 0;
            if (this._skeletonDataBlock.Buffer != null) {
                skekeletonBufferHash = (ulong) (this._skeletonDataBlock.Buffer.Id & 0xF);
            }

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
                this._skeletonDataBlock.Buffer != null ? this._skeletonDataBlock.Buffer.Id : 0,
                this._transformDataBlock.Buffer.Id
            );
        }

        private void _RecomputeOffsetVertexData()
        {
            if (! this._instanceDataBlock.IsValid) return;

            this.OffsetVertexData = new OffsetVertexData(
                this._transformDataBlock.BlockIndex, 
                this._instanceDataBlock.BlockIndex, 
                this._skeletonDataBlock.IsValid ? this._skeletonDataBlock.BlockIndex * MAX_NUMBER_OF_BONES : 0, 
                this.PickingID
            );
        }

        private void _RecomputeBoundingBox()
        {
            this._aabb = BoundingBox.Transform(this.Mesh!.AABB, this._transform);
            this._centerPosition = this._aabb.GetCenter();
        }

        internal ulong GetSortKey(Vector3 cameraPosition)
        {
            float dist = Vector3.DistanceSquared(this._centerPosition, cameraPosition);
            uint cameraDistance = Math.Min(uint.MaxValue, (uint) (dist * 1000f));
            return this._cachedSortKey | (cameraDistance & 0xFFFFFF); // 24 bits
        }
    }
}