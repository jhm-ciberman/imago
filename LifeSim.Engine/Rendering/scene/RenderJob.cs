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
        private ResourceSet? _currentInstanceResourceSet;
        private ResourceSet _passResourceSet;

        private uint[] _instanceOffsets = new uint[1];
        private uint[] _transformOffsets = new uint[1];

        private DeviceBuffer _offsetsVertexBuffer;
        private uint _maxInstancesPerBatch = 1024;

        private OffsetVertexData[] _offsetVertexData;

        private GraphicsDevice _gd;

        private uint[] _instanceRepeat;

        public RenderJob(GraphicsDevice gd, ResourceSet passResourceSet)
        {
            this._gd = gd;
            this._passResourceSet = passResourceSet;

            this._offsetsVertexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(
                16 * this._maxInstancesPerBatch, BufferUsage.VertexBuffer | BufferUsage.Dynamic
            ));

            this._offsetVertexData = new OffsetVertexData[this._maxInstancesPerBatch];
            this._instanceRepeat = new uint[this._maxInstancesPerBatch];
        }

        public void FillOffsets(CommandList commandList, IReadOnlyList<RenderItem> renderables)
        {
            if (renderables.Count == 0) return;

            ResourceSet prevInstanceRs = renderables[0].instanceResourceSet;
            ResourceSet prevMaterialRs = renderables[0].materialResourceSet;
            Mesh prevMesh = renderables[0].mesh;

            uint instanceRepeatCount = 0;
            int repeatArrayIndex = 0;
            for (int i = 0; i < renderables.Count; i++) {
                RenderItem item = renderables[i];

                this._offsetVertexData[i] = new OffsetVertexData {
                    transformDataOffset = item.transformBufferOffset / 64,
                    instanceDataOffset = item.instanceBufferOffset / 48,
                    pickingId = 0,
                };

                if (item.mesh == prevMesh && item.materialResourceSet == prevMaterialRs && item.instanceResourceSet == prevInstanceRs) {
                    instanceRepeatCount++;
                } else {
                    this._instanceRepeat[repeatArrayIndex++] = instanceRepeatCount;
                    instanceRepeatCount = 1;
                    prevInstanceRs = item.instanceResourceSet;
                    prevMaterialRs = item.materialResourceSet;
                    prevMesh = item.mesh;
                }
            }
            this._instanceRepeat[repeatArrayIndex++] = instanceRepeatCount;

            commandList.UpdateBuffer(this._offsetsVertexBuffer, 0, this._offsetVertexData);
        }

        public void DrawRenderList(CommandList commandList, ResourceSet transformsResourceSet, IReadOnlyList<RenderItem> renderables)
        {
            this._currentPipeline = null;
            this._currentMesh = null;
            this._currentMaterialResourceSet = null;
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
                    commandList.SetGraphicsResourceSet(BINDING_TRANSFORM, transformsResourceSet);
                    this._currentPipeline = pipeline;
                    this._currentMaterialResourceSet = null;
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