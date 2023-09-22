using System.Collections.Generic;
using System.Diagnostics;
using Veldrid;

namespace Imago.Rendering.Forward;

internal class RenderJob
{
    private const uint BINDING_PASS = 0;
    private const uint BINDING_TRANSFORM = 1;
    private const uint BINDING_MATERIAL = 2;
    private const uint BINDING_INSTANCE = 3;
    private const uint BINDING_SKELETON = 4;
    private readonly RenderBatcher _batcher;

    public RenderJob(GraphicsDevice gd, bool shadowmapPass)
    {
        this._batcher = new RenderBatcher(gd, shadowmapPass);
    }

    public void DrawRenderList(CommandList commandList, ResourceSet passResourceSet, IReadOnlyList<Renderable> renderItems)
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
                commandList.SetGraphicsResourceSet(BINDING_PASS, passResourceSet);
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
                Debug.Assert(batch.Mesh.VeldridVertexBuffer.IsDisposed == false);
                Debug.Assert(batch.Mesh.VeldridIndexBuffer.IsDisposed == false);
                commandList.SetVertexBuffer(1, batch.Mesh.VeldridVertexBuffer, 0);
                commandList.SetIndexBuffer(batch.Mesh.VeldridIndexBuffer, IndexFormat.UInt16);
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
    }
}
