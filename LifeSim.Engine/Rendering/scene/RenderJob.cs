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

        private void _PrepareBatches(IReadOnlyList<RenderItem> renderables)
        {
            if (renderables.Count == 0) return;

            ResourceSet prevInstanceRs = renderables[0].instanceResourceSet;
            ResourceSet prevMaterialRs = renderables[0].materialResourceSet;
            ResourceSet prevTrasnformRs = renderables[0].transformResourceSet;
            Mesh prevMesh = renderables[0].mesh;

            uint instanceRepeatCount = 0;
            int repeatArrayCount = 0;
            if (this._offsetVertexData.Length < renderables.Count) {
                Array.Resize(ref this._offsetVertexData, renderables.Count * 2);
            }
            for (int i = 0; i < renderables.Count; i++) {
                RenderItem item = renderables[i];

                this._offsetVertexData[i] = new OffsetVertexData {
                    transformDataOffset = item.transformBufferOffset,
                    instanceDataOffset = item.instanceBufferOffset,
                    pickingId = 0,
                };

                if ( // If it's batcheable, add to current batch
                    item.mesh == prevMesh 
                    && item.materialResourceSet == prevMaterialRs 
                    && item.instanceResourceSet == prevInstanceRs 
                    && item.transformResourceSet == prevTrasnformRs
                ) {
                    instanceRepeatCount++;
                } else {
                    if (repeatArrayCount == this._instanceRepeat.Length) {
                        Array.Resize(ref this._instanceRepeat, this._instanceRepeat.Length * 2);
                    }
                    this._instanceRepeat[repeatArrayCount++] = instanceRepeatCount;
                    instanceRepeatCount = 1;
                    prevInstanceRs = item.instanceResourceSet;
                    prevMaterialRs = item.materialResourceSet;
                    prevMesh = item.mesh;
                }
            }

            this._instanceRepeat[repeatArrayCount++] = instanceRepeatCount;
        }

        public void DrawRenderList(CommandList commandList, IReadOnlyList<RenderItem> renderables)
        {
            this._PrepareBatches(renderables);
            
            DeviceBuffer offsetsVertexBuffer = this._GetVertexOffsetBuffer();

            commandList.UpdateBuffer(offsetsVertexBuffer, 0, this._offsetVertexData);

            this._currentPipeline = null;
            this._currentMesh = null;
            this._currentMaterialResourceSet = null;
            this._currentTransformResourceSet = null;
            this._currentInstanceResourceSet = null;

            int instanceRepeatArrayIndex = 0;
            int drawCallCount = 0;
            uint instanceIndex = 0;

            while (instanceIndex < renderables.Count) {
                RenderItem item = renderables[(int) instanceIndex];

                var pipeline = item.shader.GetPipeline(item.mesh.vertexFormat);

                if (this._currentPipeline != pipeline) {
                    commandList.SetPipeline(pipeline);
                    commandList.SetGraphicsResourceSet(BINDING_PASS, this._passResourceSet);
                    this._currentPipeline = pipeline;
                    this._currentMaterialResourceSet = null;
                }
                
                if (this._currentMaterialResourceSet != item.transformResourceSet) {
                    commandList.SetGraphicsResourceSet(BINDING_TRANSFORM, item.transformResourceSet);
                    this._currentTransformResourceSet = item.transformResourceSet;
                }

                if (this._currentMaterialResourceSet != item.materialResourceSet) {
                    commandList.SetGraphicsResourceSet(BINDING_MATERIAL, item.materialResourceSet);
                    this._currentMaterialResourceSet = item.materialResourceSet;
                }

                if (this._currentInstanceResourceSet != item.instanceResourceSet) {
                    commandList.SetGraphicsResourceSet(BINDING_INSTANCE, item.instanceResourceSet);
                    this._currentInstanceResourceSet = item.instanceResourceSet;
                }

                if (this._currentMesh != item.mesh) {
                    commandList.SetVertexBuffer(0, item.mesh.vertexBuffer, 0);
                    commandList.SetVertexBuffer(1, this._offsetsVertexBuffer, 0);
                    commandList.SetIndexBuffer(item.mesh.indexBuffer, Veldrid.IndexFormat.UInt16);
                    this._currentMesh = item.mesh;
                }

                uint instanceRepeat = this._instanceRepeat[instanceRepeatArrayIndex++];

                commandList.DrawIndexed(
                    indexCount: item.mesh.indexCount,
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