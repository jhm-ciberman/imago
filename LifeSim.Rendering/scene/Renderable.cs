using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public class Renderable
    {
        public const int MAX_NUMBER_OF_BONES = 64;
        public int renderListIndex;
        private Matrix4x4 _transform;
        private Vector3 _centerPosition;
        private BoundingBox _aabb;
        private ulong _cachedSortKey;
        public int batchingHashKey { get; private set; }

        internal Veldrid.ResourceSet transformResourceSet { get; private set; }
        internal Veldrid.ResourceSet? materialResourceSet { get; private set; } = null;
        internal Veldrid.ResourceSet? instanceResourceSet { get; private set; } = null;
        internal Veldrid.ResourceSet? skeletonResourceSet { get; private set; }

        public OffsetVertexData offsetVertexData;

        private DataBlock _transformDataBlock;
        private DataBlock _skeletonDataBlock;

        public Mesh? mesh { get; private set; } = null;

        private uint _pickingID = 0;
        public uint pickingID
        { 
            get => this._pickingID; 
            set { this._pickingID = value; this._RecomputeOffsetVertexData(); }
        }
 
        public ISkeleton? skeleton  { get; private set; }
        public Material? material { get; private set; }
        internal DataBlock _instanceDataBlock;
        private SceneStorage _storage;

        public Renderable(SceneStorage storage)
        {
            this._storage = storage;
            
            this._transformDataBlock = storage.RequestTransformDataBlock();
            this.transformResourceSet = this._transformDataBlock.buffer.resourceSet;
        }

        public Renderable(SceneStorage storage, Mesh mesh) : this(storage)
        {
            this.SetMesh(mesh);
        }

        public void SetMesh(Mesh mesh)
        {
            this.mesh = mesh;
            this._RecomputeSortKey();
        }

        public void SetMaterial(Material material)
        {
            this.material = material;
            if (this._instanceDataBlock.blockSize != material.definition.instanceDataBlockSize) {
                this._instanceDataBlock.FreeBlock();
                this._instanceDataBlock = this._storage.RequestInstanceDataBlock(material.definition);
            }
            this.materialResourceSet = material.GetMaterialResourceSet();
            this.instanceResourceSet = this._instanceDataBlock.buffer.resourceSet;
            var span = material.definition.GetDefaultInstanceData();
            this._instanceDataBlock.WriteSpan(span);
            this._RecomputeOffsetVertexData();
            this._RecomputeSortKey();
        }

        public void SetSkeleton(ISkeleton skeleton)
        {
            if (this.skeleton == skeleton) return;
            if (! this._skeletonDataBlock.isValid) {
                this._skeletonDataBlock = this._storage.RequestSkeletonDataBlock();
                this.skeletonResourceSet = this._skeletonDataBlock.buffer.resourceSet;
                this._RecomputeOffsetVertexData();
                this._RecomputeSortKey();
            }
            this.skeleton = skeleton;
        }

        public void Update()
        {
            if (this.skeleton != null) {
                Matrix4x4.Invert(this._transform, out Matrix4x4 inverseMeshWorldMatrix);
                this.skeleton.UpdateMatrices(ref inverseMeshWorldMatrix);
                this._skeletonDataBlock.WriteSpan<Matrix4x4>(this.skeleton.bonesMatrices);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Cull(ref BoundingFrustum frustum)
        {
            return (frustum.Contains(this._aabb) != ContainmentType.Disjoint);
        }

        public void SetTransform(ref Matrix4x4 transform)
        {
            this._transform = transform;
            this._transformDataBlock.Write(ref transform);
            if (this.mesh != null) {
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
            if (this.material == null) throw new Exception("No material set");
            var offset = this.material.definition.instanceUniformData[name];
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
            if (this.material == null || this.mesh == null) return;

            ulong materialHash        = (ulong) (this.material.id & 0xFFF);
            ulong meshHash            = (ulong) (this.mesh.id & 0xFFF);
            ulong transformBufferHash = (ulong) (this._transformDataBlock.buffer.id & 0xFF);
            ulong instanceBufferHash  = (ulong) (this._instanceDataBlock.buffer.id & 0xFF);
            ulong skekeletonBufferHash = 0;
            if (this._skeletonDataBlock.buffer != null) {
                skekeletonBufferHash = (ulong) (this._skeletonDataBlock.buffer.id & 0xF);
            }

            ulong key = (materialHash   << 56) // 8 bits
                | (transformBufferHash  << 48) // 8 bits
                | (transformBufferHash  << 40) // 8 bits
                | (skekeletonBufferHash << 36) // 4 bits
                | (meshHash             << 24); // 12 bits

            this._cachedSortKey = key;

            // This key is used to fast check if two instances can be batched together. (They must have the same hash)
            this.batchingHashKey = HashCode.Combine(
                this.mesh.id, 
                this.material.id,
                this._instanceDataBlock.buffer.id, 
                this._skeletonDataBlock.buffer != null ? this._skeletonDataBlock.buffer.id : 0,
                this._transformDataBlock.buffer.id
            );
        }

        private void _RecomputeOffsetVertexData()
        {
            if (! this._instanceDataBlock.isValid) return;

            this.offsetVertexData = new OffsetVertexData(
                this._transformDataBlock.blockIndex, 
                this._instanceDataBlock.blockIndex, 
                this._skeletonDataBlock.isValid ? this._skeletonDataBlock.blockIndex * MAX_NUMBER_OF_BONES : 0, 
                this.pickingID
            );
        }

        private void _RecomputeBoundingBox()
        {
            this._aabb = BoundingBox.Transform(this.mesh!.aabb, this._transform);
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