using System;
using System.Collections.Generic;
using Veldrid;

namespace LifeSim.Rendering
{
    public class SceneStorage : IDisposable
    {
        public const int MIN_BUFFER_BLOCKS = 1024;
        private readonly GraphicsDevice _gd;
        private readonly List<DataBuffer> _instanceDataBuffers = new List<DataBuffer>();
        private readonly List<DataBuffer> _transformDataBuffers = new List<DataBuffer>();
        private readonly List<DataBuffer> _skeletonDataBuffers = new List<DataBuffer>();
        private readonly ResourceLayout _transformResourceLayout;
        private readonly ResourceLayout _instanceResourceLayout;
        private readonly ResourceLayout _skeletonResourceLayout;

        public SceneStorage(GraphicsDevice gd, ResourceLayout transformResourceLayout, ResourceLayout instanceResourceLayout, ResourceLayout sleletonResourceLayout)
        {
            this._gd = gd;

            this._transformResourceLayout = transformResourceLayout;
            this._instanceResourceLayout = instanceResourceLayout;
            this._skeletonResourceLayout = sleletonResourceLayout;
        }
        
        internal DataBlock RequestTransformDataBlock()
        {
            for (int i = 0; i < this._transformDataBuffers.Count; i++) {
                var buffer = this._transformDataBuffers[i];
                if (!buffer.IsFull) {
                    return buffer.RequestBlock();
                }
            }

            var newBuffer = new DataBuffer(this._gd, MIN_BUFFER_BLOCKS, 64, this._transformResourceLayout);
            newBuffer.Name = "TransformDataBuffer " + this._transformDataBuffers.Count;
            this._transformDataBuffers.Add(newBuffer);
            return newBuffer.RequestBlock();
        }

        internal DataBlock RequestInstanceDataBlock(MaterialDefinition material)
        {
            var blockSize = material.InstanceDataBlockSize;
            for (int i = 0; i < this._instanceDataBuffers.Count; i++) {
                var buffer = this._instanceDataBuffers[i];
                if (buffer.BlockSize == blockSize && ! buffer.IsFull) {
                    return buffer.RequestBlock();
                }
            }

            var newBuffer = new DataBuffer(this._gd, MIN_BUFFER_BLOCKS, blockSize, this._instanceResourceLayout);
            newBuffer.Name = "InstanceDataBuffer " + this._instanceDataBuffers.Count;
            this._instanceDataBuffers.Add(newBuffer);
            return newBuffer.RequestBlock();
        }

        internal DataBlock RequestSkeletonDataBlock()
        {
            for (int i = 0; i < this._skeletonDataBuffers.Count; i++) {
                var buffer = this._skeletonDataBuffers[i];
                if (! buffer.IsFull) {
                    return buffer.RequestBlock();
                }
            }

            var newBuffer = new DataBuffer(this._gd, MIN_BUFFER_BLOCKS / Renderable.MAX_NUMBER_OF_BONES, Renderable.MAX_NUMBER_OF_BONES * 64, this._skeletonResourceLayout);
            newBuffer.Name = "SkeletonDataBuffer " + this._skeletonDataBuffers.Count;
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