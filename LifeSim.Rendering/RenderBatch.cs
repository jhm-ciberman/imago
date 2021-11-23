using System.Diagnostics.Contracts;
using Veldrid;

namespace LifeSim.Rendering
{
    public readonly struct RenderBatch
    {
        public readonly uint InstanceCount { get; }
        public readonly Mesh Mesh { get; }
        public readonly Veldrid.Pipeline Pipeline { get; }
        public readonly Veldrid.ResourceSet TransformResourceSet { get; }
        public readonly Veldrid.ResourceSet MaterialResourceSet { get; }
        public readonly Veldrid.ResourceSet InstanceResourceSet { get; }
        public readonly Veldrid.ResourceSet? SkeletonResourceSet { get; }

        public RenderBatch(uint instanceCount, Renderable renderable, bool shadowmapPass)
        {
            Contract.Assume(renderable.Mesh != null);
            Contract.Assume(renderable.Material != null);
            Contract.Assume(renderable.Material.ResourceSet != null);
            Contract.Assume(renderable.InstanceResourceSet != null);

            this.InstanceCount = instanceCount;
            this.Mesh = renderable.Mesh;
            var shader = shadowmapPass ? renderable.Material.ShadowmapShader : renderable.Material.Shader;
            this.Pipeline = shader.GetPipeline(renderable.Mesh.VertexFormat);
            this.TransformResourceSet = renderable.TransformResourceSet;
            this.MaterialResourceSet = renderable.Material.ResourceSet;
            this.InstanceResourceSet = renderable.InstanceResourceSet;
            this.SkeletonResourceSet = renderable.SkeletonResourceSet;
        }
    }
}