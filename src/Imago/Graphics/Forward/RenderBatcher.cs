using System;
using System.Collections.Generic;
using System.Diagnostics;
using Imago.Graphics.Meshes;
using Veldrid;

namespace Imago.Graphics.Forward;

internal enum RenderBatchPassType
{
    Forward,
    ShadowMap,
    Picking
}

/// <summary>
/// The RenderBatcher class is responsible for batching and rendering a list of <see cref="Renderable"/> objects.
/// </summary>
internal class RenderBatcher
{
    private const uint BINDING_PASS = 0;
    private const uint BINDING_TRANSFORM = 1;
    private const uint BINDING_MATERIAL = 2;
    private const uint BINDING_INSTANCE = 3;
    private const uint BINDING_SKELETON = 4;

    private DeviceBuffer? _offsetsVertexBuffer = null;
    private OffsetVertexData[] _offsetVertexData;
    private readonly List<RenderBatch> _batches;
    private readonly GraphicsDevice _gd;
    private readonly RenderBatchPassType _pass;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderBatcher"/> class.
    /// </summary>
    /// <param name="gd">The <see cref="GraphicsDevice"/> to use for rendering.</param>
    /// <param name="pass">The <see cref="RenderBatchPassType"/> of the render pass.</param>
    public RenderBatcher(GraphicsDevice gd, RenderBatchPassType pass)
    {
        this._gd = gd;
        this._pass = pass;
        this._offsetVertexData = new OffsetVertexData[1024];
        this._batches = new List<RenderBatch>(1024);
    }

    /// <summary>
    /// Draws a list of <see cref="Renderable"/> objects using the specified <see cref="CommandList"/> and <see cref="ResourceSet"/>.
    /// </summary>
    /// <param name="commandList">The <see cref="CommandList"/> to use for rendering.</param>
    /// <param name="passResourceSet">The <see cref="ResourceSet"/> to use for the render pass.</param>
    /// <param name="renderItems">The list of <see cref="Renderable"/> objects to render.</param>
    public void DrawRenderList(CommandList commandList, ResourceSet passResourceSet, IReadOnlyList<Renderable> renderItems)
    {
        this.PrepareBatches(renderItems);

        DeviceBuffer offsetsVertexBuffer = this.GetVertexOffsetBuffer(commandList);

        Pipeline? currentPipeline = null;
        Mesh? currentMesh = null;
        ResourceSet? currentMaterialRS = null;
        ResourceSet? currentTransformRS = null;
        ResourceSet? currentInstanceRS = null;
        ResourceSet? currentSkeletonRS = null;

        uint instanceIndex = 0;

        var batches = this._batches;

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

    private void PrepareBatches(IReadOnlyList<Renderable> renderables)
    {
        this._batches.Clear();
        if (renderables.Count == 0) return;

        uint instanceCount = 0;

        if (this._offsetVertexData.Length < renderables.Count)
            Array.Resize(ref this._offsetVertexData, (int)(renderables.Count * 1.2f));
        Renderable prevRenderable = renderables[0];
        for (int i = 0; i < renderables.Count; i++)
        {
            Renderable renderable = renderables[i];
            this._offsetVertexData[i] = renderable.OffsetVertexData;

            // If it's batcheable, add to current batch. If not, finish batch
            if (renderable.CanBeBatchedWith(prevRenderable))
                instanceCount++;
            else
            {
                this._batches.Add(new RenderBatch(instanceCount, prevRenderable, this._pass));
                prevRenderable = renderable;
                instanceCount = 1;
            }
        }

        this._batches.Add(new RenderBatch(instanceCount, prevRenderable, this._pass));
    }

    private DeviceBuffer GetVertexOffsetBuffer(CommandList commandList)
    {
        uint requiredSizeInBytes = (uint) (this._offsetVertexData.Length * 16);
        if (this._offsetsVertexBuffer == null || this._offsetsVertexBuffer.SizeInBytes < requiredSizeInBytes)
        {
            if (this._offsetsVertexBuffer != null)
            {
                Renderer.Instance.DisposeWhenIdle(this._offsetsVertexBuffer);
            }

            this._offsetsVertexBuffer = this._gd.ResourceFactory.CreateBuffer(new BufferDescription(
                requiredSizeInBytes, BufferUsage.VertexBuffer | BufferUsage.Dynamic
            ));
        }

        commandList.UpdateBuffer(this._offsetsVertexBuffer, 0, this._offsetVertexData);

        return this._offsetsVertexBuffer;
    }

    private readonly struct RenderBatch
    {
        public readonly uint InstanceCount { get; }
        public readonly Mesh Mesh { get; }
        public readonly Pipeline Pipeline { get; }
        public readonly ResourceSet TransformResourceSet { get; }
        public readonly ResourceSet MaterialResourceSet { get; }
        public readonly ResourceSet InstanceResourceSet { get; }
        public readonly ResourceSet? SkeletonResourceSet { get; }

        public RenderBatch(uint instanceCount, Renderable renderable, RenderBatchPassType pass)
        {
            this.InstanceCount = instanceCount;
            this.Mesh = renderable.Mesh!;
            this.TransformResourceSet = renderable.TransformResourceSet;
            this.MaterialResourceSet = renderable.Material!.ResourceSet;
            this.InstanceResourceSet = renderable.InstanceResourceSet;
            this.SkeletonResourceSet = renderable.SkeletonResourceSet;
            this.Pipeline = pass switch
            {
                RenderBatchPassType.Forward => renderable.ForwardPipeline!,
                RenderBatchPassType.ShadowMap => renderable.ShadowMapPipeline!,
                RenderBatchPassType.Picking => renderable.PickingPipeline!,
                _ => throw new System.NotImplementedException()
            };
        }
    }
}
