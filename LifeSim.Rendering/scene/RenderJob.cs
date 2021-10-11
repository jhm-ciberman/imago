using System;
using System.Collections.Generic;
using Veldrid;

namespace LifeSim.Rendering
{
    public class RenderJob
    {
        private const uint BINDING_PASS = 0;
        private const uint BINDING_TRANSFORM = 1;
        private const uint BINDING_MATERIAL = 2;
        private const uint BINDING_INSTANCE = 3;
        private const uint BINDING_SKELETON = 4;
        private readonly ResourceSet _passResourceSet;
        private readonly RenderBatcher _batcher;

        public RenderJob(GraphicsDevice gd, ResourceSet passResourceSet, bool shadowmapPass)
        {
            this._passResourceSet = passResourceSet;
            this._batcher = new RenderBatcher(gd, shadowmapPass);
        }

        public void DrawRenderList(CommandList commandList, IReadOnlyList<Renderable> renderItems)
        {
            this._batcher.PrepareBatches(renderItems);

            DeviceBuffer offsetsVertexBuffer = this._batcher.GetVertexOffsetBuffer(commandList);

            Pipeline? currentPipeline = null;
            Mesh? currentMesh = null;
            ResourceSet? currentMaterialRS = null;
            ResourceSet? currentTransformRS = null;
            ResourceSet? currentInstanceRS = null;
            ResourceSet? currentSkeletonRS = null;

            uint instanceIndex = 0;

            var batches = this._batcher.Batches;

            commandList.SetVertexBuffer(0, offsetsVertexBuffer, 0);


            for (int batchIndex = 0; batchIndex < batches.Count; batchIndex++)
            {
                RenderBatch batch = batches[batchIndex];

                if (currentPipeline != batch.Pipeline)
                {
                    commandList.SetPipeline(batch.Pipeline);
                    commandList.SetGraphicsResourceSet(BINDING_PASS, this._passResourceSet);
                    currentPipeline = batch.Pipeline;
                    currentTransformRS = null;
                    currentMaterialRS = null;
                    currentInstanceRS = null;
                    currentSkeletonRS = null;
                }
                if (currentTransformRS != batch.TransformResourceSet)
                {
                    currentTransformRS = batch.TransformResourceSet;
                    commandList.SetGraphicsResourceSet(BINDING_TRANSFORM, batch.TransformResourceSet);
                }
                if (currentMaterialRS != batch.MaterialResourceSet)
                {
                    currentMaterialRS = batch.MaterialResourceSet;
                    commandList.SetGraphicsResourceSet(BINDING_MATERIAL, batch.MaterialResourceSet);
                }
                if (currentInstanceRS != batch.InstanceResourceSet)
                {
                    currentInstanceRS = batch.InstanceResourceSet;
                    commandList.SetGraphicsResourceSet(BINDING_INSTANCE, batch.InstanceResourceSet);
                }
                if (batch.SkeletonResourceSet != null && currentSkeletonRS != batch.SkeletonResourceSet)
                {
                    currentSkeletonRS = batch.SkeletonResourceSet;
                    commandList.SetGraphicsResourceSet(BINDING_SKELETON, batch.SkeletonResourceSet);
                }

                if (currentMesh != batch.Mesh)
                {
                    commandList.SetVertexBuffer(1, batch.Mesh.VertexBuffer, 0);
                    commandList.SetIndexBuffer(batch.Mesh.IndexBuffer, Veldrid.IndexFormat.UInt16);
                    currentMesh = batch.Mesh;
                }

                commandList.DrawIndexed(
                    indexCount: batch.Mesh.IndexCount,
                    instanceCount: batch.InstanceCount,
                    indexStart: 0,
                    vertexOffset: 0,
                    instanceStart: instanceIndex
                );

                instanceIndex += batch.InstanceCount;
            }

            //Console.WriteLine("DrawCalls: " + drawCallCount);
        }
    }
}