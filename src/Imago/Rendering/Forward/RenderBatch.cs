using System.Diagnostics.Contracts;
using Veldrid;

namespace Imago.Rendering.Forward;

internal readonly struct RenderBatch
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
