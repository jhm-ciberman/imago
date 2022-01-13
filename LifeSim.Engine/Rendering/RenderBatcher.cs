using System;
using System.Collections.Generic;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class RenderBatcher
{
    private DeviceBuffer? _offsetsVertexBuffer = null;
    private OffsetVertexData[] _offsetVertexData;
    private readonly List<RenderBatch> _batches;

    public IReadOnlyList<RenderBatch> Batches => this._batches;

    private readonly GraphicsDevice _gd;
    private readonly bool _shadowMapPass;

    public RenderBatcher(GraphicsDevice gd, bool shadowMapPass)
    {
        this._gd = gd;
        this._shadowMapPass = shadowMapPass;
        this._offsetVertexData = new OffsetVertexData[1024];
        this._batches = new List<RenderBatch>(1024);
    }

    public void PrepareBatches(IReadOnlyList<Renderable> renderables)
    {
        this._batches.Clear();
        if (renderables.Count == 0) return;

        uint instanceCount = 0;

        if (this._offsetVertexData.Length < renderables.Count)
        {
            Array.Resize(ref this._offsetVertexData, (int)(renderables.Count * 1.2f));
        }
        Renderable prevRenderable = renderables[0];
        int prevBatchingHashKey = prevRenderable.BatchingHashKey;
        for (int i = 0; i < renderables.Count; i++)
        {
            Renderable renderable = renderables[i];
            this._offsetVertexData[i] = renderable.OffsetVertexData;

            // If it's batcheable, add to current batch. If not, finish batch
            if (renderable.BatchingHashKey == prevBatchingHashKey)
            {
                instanceCount++;
            }
            else
            {
                this._batches.Add(new RenderBatch(instanceCount, prevRenderable, this._shadowMapPass));
                prevRenderable = renderable;
                instanceCount = 1;
                prevBatchingHashKey = renderable.BatchingHashKey;
            }
        }

        this._batches.Add(new RenderBatch(instanceCount, prevRenderable, this._shadowMapPass));
    }

    public DeviceBuffer GetVertexOffsetBuffer(CommandList commandList)
    {
        uint requiredSizeInBytes = (uint) (this._offsetVertexData.Length * 16);
        if (this._offsetsVertexBuffer == null || this._offsetsVertexBuffer.SizeInBytes < requiredSizeInBytes)
        {
            if (this._offsetsVertexBuffer != null)
            {
                this._gd.DisposeWhenIdle(this._offsetsVertexBuffer);
            }
            this._offsetsVertexBuffer = this._gd.ResourceFactory.CreateBuffer(new BufferDescription(
                requiredSizeInBytes, BufferUsage.VertexBuffer | BufferUsage.Dynamic
            ));
        }

        commandList.UpdateBuffer(this._offsetsVertexBuffer, 0, this._offsetVertexData);

        return this._offsetsVertexBuffer;
    }

}