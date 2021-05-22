using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Core;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class SceneStorage : IDisposable
    {
        public const int MAX_INSTANCE_UNIFORM_INDICES = 16;

        public const int MIN_BUFFER_BLOCKS = 4096;

        private DataBuffer?[] _buffers;

        private SwapPopList<Renderable> _renderables = new SwapPopList<Renderable>();
        public IReadOnlyList<Renderable> renderables => this._renderables;
        private GraphicsDevice _gd;

        private DataBuffer _tranformDataBuffer;
        private Skeleton _bonesInfo = new Skeleton();
        private readonly ResourceLayout _transformResourceLayout;
        private readonly ResourceLayout _instanceResourceLayout;
        private readonly ResourceLayout _bonesResourceLayout;

        //private readonly DeviceBuffer _bonesInfoBuffer;
        //private readonly ResourceSet _skinnedResourceSet;

        public SceneStorage(GraphicsDevice gd, ResourceLayout transformResourceLayout, ResourceLayout instanceResourceLayout, ResourceLayout bonesResourceLayout)
        {
            this._gd = gd;
            this._buffers = new DataBuffer?[MAX_INSTANCE_UNIFORM_INDICES];
            var factory = gd.ResourceFactory;

            this._transformResourceLayout = transformResourceLayout;
            this._instanceResourceLayout = instanceResourceLayout;
            this._bonesResourceLayout = bonesResourceLayout;

            this._tranformDataBuffer = new DataBuffer(this._gd, MIN_BUFFER_BLOCKS, Marshal.SizeOf<Matrix4x4>(), this._transformResourceLayout);
            this._tranformDataBuffer.name = "Transforms Buffer";
            //this._bonesInfoBuffer = factory.CreateBuffer(new BufferDescription(64 * Skeleton.MAX_NUMBER_OF_BONES, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            //this._skinnedResourceSet = factory.CreateResourceSet(new ResourceSetDescription(skinnedMeshLayout, this._modelInfoBuffer, this._bonesInfoBuffer));

        }

        public Veldrid.ResourceSet transformsResourceSet => this._tranformDataBuffer.resourceSet;

        public Renderable CreateRenderable(Mesh mesh, SurfaceMaterial material)
        {
            var instanceDataBuffer = this._GetInstanceDataBuffer(material.shader);
            var renderable = new Renderable(mesh, material, instanceDataBuffer, this._tranformDataBuffer);
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

        private DataBuffer _GetInstanceDataBuffer(Shader shader)
        {
            var indicesCount = shader.instanceUniformData.Count;
            var buffer = this._buffers[indicesCount];
            if (buffer == null) {
                System.Console.WriteLine("Creating Instance data buffer: " + indicesCount);
                buffer = new DataBuffer(this._gd, MIN_BUFFER_BLOCKS, indicesCount * 16, this._instanceResourceLayout);
                buffer.name = "InstanceData Buffer ("+indicesCount+" uniforms)";
                this._buffers[indicesCount] = buffer;
            }
            return buffer;
        }

        public void UpdateBuffers(Veldrid.CommandList commandList)
        {
            this._tranformDataBuffer.UploadToGPU(commandList);
            for (int i = 0; i < this._buffers.Length; i++) {
                this._buffers[i]?.UploadToGPU(commandList);
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < this._buffers.Length; i++) {
                this._buffers[i]?.Dispose();
            }

            this._tranformDataBuffer.Dispose();
        }
    }
}