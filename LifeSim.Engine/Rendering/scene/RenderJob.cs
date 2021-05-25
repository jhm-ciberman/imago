using System;
using System.Collections.Generic;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class RenderJob
    {
        struct OffsetVertexData // It's 16 bytes only! 
        {
            public uint transformDataOffset; // x
            public uint instanceDataOffset; // y
            public uint pickingId; // z
            public readonly uint _padding; // w
        }

        const uint BINDING_PASS = 0;
        const uint BINDING_TRANSFORM = 1;
        const uint BINDING_MATERIAL = 2;
        const uint BINDING_INSTANCE = 3;

        private Pipeline? _currentPipeline;
        private Mesh? _currentMesh;
        private ResourceSet? _currentMaterialResourceSet;
        private ResourceSet? _currentTransformResourceSet;
        private ResourceSet? _currentInstanceResourceSet;
        private ResourceSet _passResourceSet;
        private DeviceBuffer? _offsetsVertexBuffer;

        private OffsetVertexData[] _offsetVertexData;

        private GraphicsDevice _gd;

        private uint[] _instanceRepeat;

        public RenderJob(GraphicsDevice gd, ResourceSet passResourceSet)
        {
            this._gd = gd;
            this._passResourceSet = passResourceSet;

            this._offsetVertexData = new OffsetVertexData[1024];
            this._instanceRepeat = new uint[1024];
        }

        private DeviceBuffer _GetVertexOffsetBuffer()
        {
            uint requiredSizeInBytes = (uint) (this._offsetVertexData.Length * 16);
            if (this._offsetsVertexBuffer == null || this._offsetsVertexBuffer.SizeInBytes < requiredSizeInBytes) {
                if (this._offsetsVertexBuffer != null) {
                    this._gd.DisposeWhenIdle(this._offsetsVertexBuffer);
                }
                this._offsetsVertexBuffer = this._gd.ResourceFactory.CreateBuffer(new BufferDescription(
                    requiredSizeInBytes, BufferUsage.VertexBuffer | BufferUsage.Dynamic
                ));
            }

            return this._offsetsVertexBuffer;
        }

        private void _PrepareBatches(IReadOnlyList<Renderable> renderables)
        {
            if (renderables.Count == 0) return;

            int prevVatchingHashKey = renderables[0].batchingHashKey;
            uint instanceRepeatCount = 0;
            int repeatArrayCount = 0;
            if (this._offsetVertexData.Length < renderables.Count) {
                Array.Resize(ref this._offsetVertexData, (int) (renderables.Count * 1.2f));
            }
            if (this._instanceRepeat.Length < renderables.Count) { // Ensure capacity for worst case scenario, nothing batcheable
                Array.Resize(ref this._instanceRepeat, (int) (renderables.Count * 1.2f));
            }
            for (int i = 0; i < renderables.Count; i++) {
                Renderable renderable = renderables[i];

                this._offsetVertexData[i] = new OffsetVertexData {
                    transformDataOffset = renderable.transformDataBlock.offset,
                    instanceDataOffset = renderable.instanceDataBlock.offset,
                    pickingId = renderable.pickingID,
                };

                // If it's batcheable, add to current batch
                if (renderable.batchingHashKey == prevVatchingHashKey) {
                    instanceRepeatCount++;
                } else {
                    this._instanceRepeat[repeatArrayCount++] = instanceRepeatCount;
                    instanceRepeatCount = 1;
                    prevVatchingHashKey = renderable.batchingHashKey;
                }
            }

            this._instanceRepeat[repeatArrayCount++] = instanceRepeatCount;
        }

        public void DrawRenderList(CommandList commandList, IReadOnlyList<Renderable> renderables, bool shadowMapPass)
        {
            this._PrepareBatches(renderables);
            
            DeviceBuffer offsetsVertexBuffer = this._GetVertexOffsetBuffer();

            this._gd.UpdateBuffer(offsetsVertexBuffer, 0, this._offsetVertexData);

            this._currentPipeline = null;
            this._currentMesh = null;
            this._currentMaterialResourceSet = null;
            this._currentTransformResourceSet = null;
            this._currentInstanceResourceSet = null;

            int instanceRepeatArrayIndex = 0;
            int drawCallCount = 0;
            uint instanceIndex = 0;

            while (instanceIndex < renderables.Count) {
                Renderable renderable = renderables[(int) instanceIndex];

                var shader = shadowMapPass ? renderable.material.shadowmapShader : renderable.material.shader;
                var pipeline = shader.GetPipeline(renderable.mesh.vertexFormat);

                if (this._currentPipeline != pipeline) {
                    commandList.SetPipeline(pipeline);
                    commandList.SetGraphicsResourceSet(BINDING_PASS, this._passResourceSet);
                    this._currentPipeline = pipeline;
                    this._currentMaterialResourceSet = null;
                    this._currentTransformResourceSet = null;
                    this._currentInstanceResourceSet = null;
                }
                
                if (this._currentTransformResourceSet != renderable.transformResourceSet) {
                    commandList.SetGraphicsResourceSet(BINDING_TRANSFORM, renderable.transformResourceSet);
                    this._currentTransformResourceSet = renderable.transformResourceSet;
                }

                if (this._currentMaterialResourceSet != renderable.materialResourceSet) {
                    commandList.SetGraphicsResourceSet(BINDING_MATERIAL, renderable.materialResourceSet);
                    this._currentMaterialResourceSet = renderable.materialResourceSet;
                }

                if (this._currentInstanceResourceSet != renderable.instanceResourceSet) {
                    commandList.SetGraphicsResourceSet(BINDING_INSTANCE, renderable.instanceResourceSet);
                    this._currentInstanceResourceSet = renderable.instanceResourceSet;
                }

                if (this._currentMesh != renderable.mesh) {
                    commandList.SetVertexBuffer(0, renderable.mesh.vertexBuffer, 0);
                    commandList.SetVertexBuffer(1, this._offsetsVertexBuffer, 0);
                    commandList.SetIndexBuffer(renderable.mesh.indexBuffer, Veldrid.IndexFormat.UInt16);
                    this._currentMesh = renderable.mesh;
                }

                uint instanceRepeat = this._instanceRepeat[instanceRepeatArrayIndex++];

                commandList.DrawIndexed(
                    indexCount: renderable.mesh.indexCount,
                    instanceCount: instanceRepeat,
                    indexStart: 0,
                    vertexOffset: 0,
                    instanceStart: instanceIndex
                );

                instanceIndex += instanceRepeat;
                drawCallCount++;
            }

            //Console.WriteLine("DrawCalls: " + drawCallCount);
        }
    }
}