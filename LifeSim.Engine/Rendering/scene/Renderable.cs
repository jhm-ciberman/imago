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

        public Veldrid.ResourceSet materialResourceSet { get; private set; }
        public Veldrid.ResourceSet instanceResourceSet { get; private set; }

        public DataBuffer.Block transformDataBlock;
        public DataBuffer.Block instanceDataBlock;
        public Mesh mesh { get; private set; }

        public Skeleton? skeleton  { get; private set; }

        public SurfaceMaterial material { get; private set; }

        public Renderable(Mesh mesh, SurfaceMaterial material, DataBuffer instanceDataBuffer, DataBuffer transformDataBuffer)
        {
            this.mesh = mesh;
            this.material = material;
            this.skeleton = null;
            
            this.instanceDataBlock = instanceDataBuffer.RequestBlock();
            this.transformDataBlock = transformDataBuffer.RequestBlock();

            this.instanceResourceSet = instanceDataBuffer.resourceSet;
            instanceDataBuffer.onResourceSetChanged += this._OnInstanceDataResourceSetChanged;

            this.materialResourceSet = material.GetMaterialResourceSet();
            this._cachedSortKey = this._RecomputeSortKey();
        }

        private void _OnInstanceDataResourceSetChanged()
        {
            this.instanceResourceSet = this.instanceDataBlock.buffer.resourceSet;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Cull(ref BoundingFrustum frustum)
        {
            return (frustum.Contains(this._aabb) != ContainmentType.Disjoint);
        }

        public void UpdateTransform(ref Matrix4x4 transform)
        {
            this._transform = transform;
            this._aabb = BoundingBox.Transform(this.mesh.aabb, this._transform);
            this._centerPosition = this._aabb.GetCenter();
        }

        public void SetInstanceData<T>(ref T data) where T : struct
        {
            this.instanceDataBlock.Write(ref data);
        }

        public void Free()
        {
            this.instanceDataBlock.FreeBlock();
            this.transformDataBlock.FreeBlock();
        }

        protected ulong _RecomputeSortKey()
        {
            ulong materialHash = (ulong) (this.material.shader.GetHashCode() & 0xFFFF);
            ulong meshHash     = (ulong) (this.mesh.GetHashCode() & 0xFFFF);
            return (materialHash << 48) | (meshHash << 32);
        }

        public ulong GetSortKey(Vector3 cameraPosition)
        {
            float dist = Vector3.DistanceSquared(this._centerPosition, cameraPosition);
            uint cameraDistance = Math.Min(uint.MaxValue, (uint) (dist * 1000f));
            return this._cachedSortKey | (cameraDistance & 0xFFFFFFFF);
        }
    }
}