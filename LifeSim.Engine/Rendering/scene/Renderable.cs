using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public class Renderable
    {
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

        private DataBuffer.Block _transformDataBlock;
        private DataBuffer.Block _skeletonDataBlock;

        public Mesh? mesh { get; private set; } = null;

        private uint _pickingID = 0;
        public uint pickingID
        { 
            get => this._pickingID; 
            set { this._pickingID = value; this._RecomputeOffsetVertexData(); }
        }
 
        public Skeleton? skeleton  { get; private set; }
        public SurfaceMaterial? material { get; private set; }
        internal DataBuffer.Block _instanceDataBlock;

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
            if (this.mesh != null && this.material != null) {
                this._RecomputeBatchingHashKey();
            }
        }

        public void SetMaterial(SurfaceMaterial material)
        {
            this.material = material;
            if (this._instanceDataBlock.blockSize != material.shader.instanceUniformData.Count * 16) {
                this._instanceDataBlock.FreeBlock();
                this._instanceDataBlock = this._storage.RequestInstanceDataBlock(material);
            }
            this.materialResourceSet = material.GetMaterialResourceSet();
            this.instanceResourceSet = this._instanceDataBlock.buffer.resourceSet;
            material.SetDefaultInstanceData(this);
            this._RecomputeOffsetVertexData();
            this._RecomputeBatchingHashKey();
            this._RecomputeSortKey();
        }

        public void SetSkeleton(Skeleton skeleton)
        {
            if (this.skeleton == skeleton) return;
            if (this.skeleton == null) {
                this._skeletonDataBlock = this._storage.RequestSkeletonDataBlock();
                this.skeletonResourceSet = this._skeletonDataBlock.buffer.resourceSet;
                this._RecomputeOffsetVertexData();
            }
            this.skeleton = skeleton;
        }

        public void Update()
        {
            if (this.skeleton != null) {
                this._UpdateSkeletonMatrices(this.skeleton);
            }
        }
        private void _UpdateSkeletonMatrices(Skeleton skeleton)
        {
            Matrix4x4.Invert(this._transform, out Matrix4x4 inverseMeshWorldMatrix);
            var joints = skeleton.joints;
            var invBindMatrices = skeleton.inverseBindMatrices;
            Span<Matrix4x4> bonesMatrices = stackalloc Matrix4x4[joints.Count];
            for (int i = 0; i < joints.Count; i++) {
                bonesMatrices[i] = invBindMatrices[i] * joints[i].worldMatrix * inverseMeshWorldMatrix;
            }
            this._skeletonDataBlock.WriteSpan(bonesMatrices);
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
            var offset = this.material.shader.instanceUniformData[name];
            this._instanceDataBlock.Write(offset, ref data);
        }

        public void Free()
        {
            this._instanceDataBlock.FreeBlock();
            this._transformDataBlock.FreeBlock();
            if (this.skeleton != null) {
                this._skeletonDataBlock.FreeBlock();
            }
        }

        protected void _RecomputeSortKey()
        {
            ulong materialHash        = (ulong) (this.material!.id & 0xFFF);
            ulong meshHash            = (ulong) (this.mesh!.id & 0xFFF);
            //ulong transformBufferHash = (ulong) (this._transformDataBlock.buffer.id & 0xF);
            ulong key = (materialHash       << 48)  // 16 bits
                | (meshHash            << 32); // 16 bits
                //| (transformBufferHash << 32); // 4 bits


            this._cachedSortKey = key;
        }

        private void _RecomputeBatchingHashKey()
        {
            // This key is used to fast check if two instances can be batched together. (They must have the same hash)
            this.batchingHashKey = HashCode.Combine(
                this.mesh!.id, 
                this.material!.id,
                this._instanceDataBlock.buffer.id, 
                this._transformDataBlock.buffer.id
            );
        }

        private void _RecomputeOffsetVertexData()
        {
            this.offsetVertexData = new OffsetVertexData(
                this._transformDataBlock.blockIndex, 
                this._instanceDataBlock.blockIndex, 
                this._skeletonDataBlock.isValid ? this._skeletonDataBlock.blockIndex : 0, 
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
            return this._cachedSortKey | (cameraDistance & 0xFFFFFFFF);
        }
    }
}