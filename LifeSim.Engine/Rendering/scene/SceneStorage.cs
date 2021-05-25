using System;
using System.Collections.Generic;
using LifeSim.Core;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class SceneStorage : IDisposable
    {
        public const int MIN_BUFFER_BLOCKS = 1024;

        private SwapPopList<Renderable> _renderables = new SwapPopList<Renderable>();
        public IReadOnlyList<Renderable> renderables => this._renderables;
        private GraphicsDevice _gd;
        private List<DataBuffer> _instanceDataBuffers = new List<DataBuffer>();
        private List<DataBuffer> _transformDataBuffers = new List<DataBuffer>();
        private Skeleton _bonesInfo = new Skeleton();
        private readonly ResourceLayout _transformResourceLayout;
        private readonly ResourceLayout _instanceResourceLayout;
        private readonly ResourceLayout _bonesResourceLayout;

        //private readonly DeviceBuffer _bonesInfoBuffer;
        //private readonly ResourceSet _skinnedResourceSet;

        public SceneStorage(GraphicsDevice gd, ResourceLayout transformResourceLayout, ResourceLayout instanceResourceLayout, ResourceLayout bonesResourceLayout)
        {
            this._gd = gd;
            var factory = gd.ResourceFactory;

            this._transformResourceLayout = transformResourceLayout;
            this._instanceResourceLayout = instanceResourceLayout;
            this._bonesResourceLayout = bonesResourceLayout;

            //this._bonesInfoBuffer = factory.CreateBuffer(new BufferDescription(64 * Skeleton.MAX_NUMBER_OF_BONES, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            //this._skinnedResourceSet = factory.CreateResourceSet(new ResourceSetDescription(skinnedMeshLayout, this._modelInfoBuffer, this._bonesInfoBuffer));

        }

        public Renderable CreateRenderable(Mesh mesh, SurfaceMaterial material)
        {
            var transformDataBuffer = this._GetTransformDataBuffer();
            var instanceDataBuffer = this._GetInstanceDataBuffer(material.shader);

            var renderable = new Renderable(mesh, material, instanceDataBuffer, transformDataBuffer);
            renderable.renderListIndex = this._renderables.Count;
            this._renderables.Add(renderable);
            return renderable;
        }

        public void RemoveRenderable(Renderable renderable)
        {
            this._renderables[this._renderables.Count - 1].renderListIndex = renderable.renderListIndex;
            this._renderables.RemoveAt(renderable.renderListIndex);
            renderable.Free();
        }

        private DataBuffer _GetTransformDataBuffer()
        {
            for (int i = 0; i < this._transformDataBuffers.Count; i++) {
                var buffer = this._transformDataBuffers[i];
                if (! buffer.isFull) {
                    return buffer;
                }
            }

            System.Console.WriteLine("Creating Transform data buffer");
            var newBuffer = new DataBuffer(this._gd, MIN_BUFFER_BLOCKS, 64, this._transformResourceLayout);
            newBuffer.name = "TransformDataBuffer";
            this._transformDataBuffers.Add(newBuffer);
            return newBuffer;
        }

        private DataBuffer _GetInstanceDataBuffer(Shader shader)
        {
            var blockSize = shader.instanceUniformData.Count * 16;
            for (int i = 0; i < this._instanceDataBuffers.Count; i++) {
                var buffer = this._instanceDataBuffers[i];
                if (buffer.blockSize == blockSize && ! buffer.isFull) {
                    return buffer;
                }
            }

            System.Console.WriteLine("Creating Instance data buffer: " + blockSize);
            var newBuffer = new DataBuffer(this._gd, MIN_BUFFER_BLOCKS, blockSize, this._instanceResourceLayout);
            newBuffer.name = "InstanceDataBuffer";
            this._instanceDataBuffers.Add(newBuffer);
            return newBuffer;
        }

        public void UpdateBuffers(Veldrid.CommandList commandList)
        {
            for (int i = 0; i < this._instanceDataBuffers.Count; i++) {
                this._instanceDataBuffers[i].UploadToGPU(commandList);
            }
            for (int i = 0; i < this._transformDataBuffers.Count; i++) {
                this._transformDataBuffers[i].UploadToGPU(commandList);
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
        }
    }
}