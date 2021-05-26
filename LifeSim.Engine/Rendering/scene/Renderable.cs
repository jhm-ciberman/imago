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

        public Veldrid.ResourceSet materialResourceSet { get; private set; }
        public Veldrid.ResourceSet instanceResourceSet { get; private set; }
        public Veldrid.ResourceSet transformResourceSet { get; private set; }
        public Veldrid.ResourceSet? skeletonResourceSet { get; private set; }

        public OffsetVertexData offsetVertexData;

        private DataBuffer.Block _transformDataBlock;
        private DataBuffer.Block _instanceDataBlock;
        private DataBuffer.Block _skeletonDataBlock;

        public Mesh mesh { get; private set; }
        public uint pickingID { get; set; } = 0;
 
        public Skeleton? skeleton  { get; private set; }

        public SurfaceMaterial material { get; private set; }

        public Renderable(SceneStorage storage, Mesh mesh, SurfaceMaterial material)
        {
            this.mesh = mesh;
            this.material = material;
            this.skeleton = null;
            
            this.materialResourceSet = material.GetMaterialResourceSet();

            this._instanceDataBlock = storage.RequestInstanceDataBlock(material.shader);
            this._transformDataBlock = storage.RequestTransformDataBlock();

            this.instanceResourceSet = this._instanceDataBlock.buffer.resourceSet;
            this.transformResourceSet = this._transformDataBlock.buffer.resourceSet;

            this._cachedSortKey = this._RecomputeSortKey();
            this.batchingHashKey = this._RecomputeBatchingHashKey();

            this.offsetVertexData = new OffsetVertexData(this._transformDataBlock.offset, this._instanceDataBlock.offset, this._skeletonDataBlock.offset, this.pickingID);
        }

        public void SetSkeleton(Skeleton skeleton, DataBuffer boneDataBuffer)
        {
            this._skeletonDataBlock = boneDataBuffer.RequestBlock();
            this.skeletonResourceSet = boneDataBuffer.resourceSet;
            this.skeleton = skeleton;
            this.offsetVertexData = new OffsetVertexData(this._transformDataBlock.offset, this._instanceDataBlock.offset, this._skeletonDataBlock.offset, this.pickingID);
        }

        private void _OnInstanceDataResourceSetChanged()
        {
            this.instanceResourceSet = this._instanceDataBlock.buffer.resourceSet;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Cull(ref BoundingFrustum frustum)
        {
            return (frustum.Contains(this._aabb) != ContainmentType.Disjoint);
        }

        public void UpdateTransform(ref Matrix4x4 transform)
        {
            this._transform = transform;
            this._transformDataBlock.Write(ref transform);
            this._aabb = BoundingBox.Transform(this.mesh.aabb, this._transform);
            this._centerPosition = this._aabb.GetCenter();
        }

        public void SetInstanceData<T>(ref T data) where T : struct
        {
            this._instanceDataBlock.Write(ref data);
        }

        public void Free()
        {
            this._instanceDataBlock.FreeBlock();
            this._transformDataBlock.FreeBlock();
            if (this.skeleton != null) {
                this._skeletonDataBlock.FreeBlock();
            }
        }

        protected ulong _RecomputeSortKey()
        {
            ulong materialHash        = (ulong) (this.material.id & 0xFFFF);
            ulong meshHash            = (ulong) (this.mesh.id & 0xFFF);
            ulong transformBufferHash = (ulong) (this._transformDataBlock.buffer.id & 0xF);
            ulong key = (materialHash << 48) | (meshHash << 36) | (transformBufferHash << 32);
            return key;
        }

        private int _RecomputeBatchingHashKey()
        {
            return HashCode.Combine(
                this.mesh.id, 
                this.materialResourceSet,
                this.instanceResourceSet, 
                this.transformResourceSet
            );
        }

        public ulong GetSortKey(Vector3 cameraPosition)
        {
            float dist = Vector3.DistanceSquared(this._centerPosition, cameraPosition);
            uint cameraDistance = Math.Min(uint.MaxValue, (uint) (dist * 1000f));
            return this._cachedSortKey | (cameraDistance & 0xFFFFFFFF);
        }
    }
}