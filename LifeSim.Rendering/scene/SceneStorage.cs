using System;
using System.Collections.Generic;
using Veldrid;

namespace LifeSim.Rendering
{
    public class SceneStorage : IDisposable
    {
        public const int MIN_BUFFER_BLOCKS = 1024;
        private GraphicsDevice _gd;
        private List<DataBuffer> _instanceDataBuffers = new List<DataBuffer>();
        private List<DataBuffer> _transformDataBuffers = new List<DataBuffer>();
        private List<DataBuffer> _skeletonDataBuffers = new List<DataBuffer>();
        private readonly ResourceLayout _transformResourceLayout;
        private readonly ResourceLayout _instanceResourceLayout;
        private readonly ResourceLayout _skeletonResourceLayout;

        public SceneStorage(GraphicsDevice gd, ResourceLayout transformResourceLayout, ResourceLayout instanceResourceLayout, ResourceLayout sleletonResourceLayout)
        {
            this._gd = gd;
            var factory = gd.ResourceFactory;

            this._transformResourceLayout = transformResourceLayout;
            this._instanceResourceLayout = instanceResourceLayout;
            this._skeletonResourceLayout = sleletonResourceLayout;
        }
        
        internal DataBlock RequestTransformDataBlock()
        {
            for (int i = 0; i < this._transformDataBuffers.Count; i++) {
                var buffer = this._transformDataBuffers[i];
                if (! buffer.isFull) {
                    return buffer.RequestBlock();
                }
            }

            var newBuffer = new DataBuffer(this._gd, MIN_BUFFER_BLOCKS, 64, this._transformResourceLayout);
            newBuffer.name = "TransformDataBuffer " + this._transformDataBuffers.Count;
            this._transformDataBuffers.Add(newBuffer);
            return newBuffer.RequestBlock();
        }

        internal DataBlock RequestInstanceDataBlock(MaterialDefinition material)
        {
            var blockSize = material.instanceDataBlockSize;
            for (int i = 0; i < this._instanceDataBuffers.Count; i++) {
                var buffer = this._instanceDataBuffers[i];
                if (buffer.blockSize == blockSize && ! buffer.isFull) {
                    return buffer.RequestBlock();
                }
            }

            var newBuffer = new DataBuffer(this._gd, MIN_BUFFER_BLOCKS, blockSize, this._instanceResourceLayout);
            newBuffer.name = "InstanceDataBuffer " + this._instanceDataBuffers.Count;
            this._instanceDataBuffers.Add(newBuffer);
            return newBuffer.RequestBlock();
        }

        internal DataBlock RequestSkeletonDataBlock()
        {
            for (int i = 0; i < this._skeletonDataBuffers.Count; i++) {
                var buffer = this._skeletonDataBuffers[i];
                if (! buffer.isFull) {
                    return buffer.RequestBlock();
                }
            }

            var newBuffer = new DataBuffer(this._gd, MIN_BUFFER_BLOCKS / Renderable.MAX_NUMBER_OF_BONES, Renderable.MAX_NUMBER_OF_BONES * 64, this._skeletonResourceLayout);
            newBuffer.name = "SkeletonDataBuffer " + this._skeletonDataBuffers.Count;
            this._skeletonDataBuffers.Add(newBuffer);
            return newBuffer.RequestBlock();
        }

        internal void UpdateBuffers(Veldrid.CommandList commandList)
        {
            for (int i = 0; i < this._instanceDataBuffers.Count; i++) {
                this._instanceDataBuffers[i].UploadToGPU(commandList);
            }
            for (int i = 0; i < this._transformDataBuffers.Count; i++) {
                this._transformDataBuffers[i].UploadToGPU(commandList);
            }
            for (int i = 0; i < this._skeletonDataBuffers.Count; i++) {
                this._skeletonDataBuffers[i].UploadToGPU(commandList);
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < this._instanceDataBuffers.Count; i++) {
                this._instanceDataBuffers[i].Dispose();
            }
            for (int i = 0; i < this._transformDataBuffers.Count; i++) {
                this._transformDataBuffers[i].Dispose();
            }
            for (int i = 0; i < this._skeletonDataBuffers.Count; i++) {
                this._skeletonDataBuffers[i].Dispose();
            }
        }
    }
}