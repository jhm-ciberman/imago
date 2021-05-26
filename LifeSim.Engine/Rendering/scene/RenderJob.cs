using System;
using System.Collections.Generic;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class RenderJob
    {
        const uint BINDING_PASS = 0;
        const uint BINDING_TRANSFORM = 1;
        const uint BINDING_MATERIAL = 2;
        const uint BINDING_INSTANCE = 3;
        private ResourceSet _passResourceSet;

        private RenderBatcher _batcher;

        public RenderJob(GraphicsDevice gd, ResourceSet passResourceSet, bool shadowmapPass)
        {
            this._passResourceSet = passResourceSet;
            this._batcher = new RenderBatcher(gd, shadowmapPass);
        }

        public void DrawRenderList(CommandList commandList, IReadOnlyList<Renderable> renderItems)
        {
            this._batcher.PrepareBatches(renderItems);
            
            DeviceBuffer offsetsVertexBuffer = this._batcher.GetVertexOffsetBuffer();

            Pipeline? currentPipeline = null;
            Mesh? currentMesh = null;
            ResourceSet? currentMaterialRS = null;
            ResourceSet? currentTransformRS = null;
            ResourceSet? currentInstanceRS = null;

            uint instanceIndex = 0;

            var batches = this._batcher.batches;

            
            for (int batchIndex = 0; batchIndex < batches.Count; batchIndex++) {
                RenderBatch batch = batches[batchIndex];

                if (currentPipeline != batch.pipeline) {
                    commandList.SetPipeline(batch.pipeline);
                    commandList.SetGraphicsResourceSet(BINDING_PASS, this._passResourceSet);
                    currentPipeline = batch.pipeline;
                    currentMaterialRS = null;
                    currentTransformRS = null;
                    currentInstanceRS = null;
                }
                
                if (currentTransformRS != batch.transformResourceSet) {
                    commandList.SetGraphicsResourceSet(BINDING_TRANSFORM, batch.transformResourceSet);
                    currentTransformRS = batch.transformResourceSet;
                }

                if (currentMaterialRS != batch.materialResourceSet) {
                    commandList.SetGraphicsResourceSet(BINDING_MATERIAL, batch.materialResourceSet);
                    currentMaterialRS = batch.materialResourceSet;
                }

                if (currentInstanceRS != batch.instanceResourceSet) {
                    commandList.SetGraphicsResourceSet(BINDING_INSTANCE, batch.instanceResourceSet);
                    currentInstanceRS = batch.instanceResourceSet;
                }

                if (currentMesh != batch.mesh) {
                    commandList.SetVertexBuffer(0, batch.mesh.vertexBuffer, 0);
                    commandList.SetVertexBuffer(1, offsetsVertexBuffer, 0);
                    commandList.SetIndexBuffer(batch.mesh.indexBuffer, Veldrid.IndexFormat.UInt16);
                    currentMesh = batch.mesh;
                }

                commandList.DrawIndexed(
                    indexCount: batch.mesh.indexCount,
                    instanceCount: batch.instanceCount,
                    indexStart: 0,
                    vertexOffset: 0,
                    instanceStart: instanceIndex
                );

                instanceIndex += batch.instanceCount;
            }

            //Console.WriteLine("DrawCalls: " + drawCallCount);
        }
    }
}