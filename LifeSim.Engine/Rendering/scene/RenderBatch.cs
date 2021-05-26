using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public readonly struct RenderBatch
    {
        public readonly uint instanceCount;
        public readonly Mesh mesh;
        public readonly Veldrid.Pipeline pipeline;
        public readonly Veldrid.ResourceSet transformResourceSet;
        public readonly Veldrid.ResourceSet materialResourceSet;
        public readonly Veldrid.ResourceSet instanceResourceSet;
        public readonly Veldrid.ResourceSet? skeletonResourceSet;

        public RenderBatch(uint instanceCount, Renderable renderable, bool shadowmapPass)
        {
            this.instanceCount = instanceCount;
            this.mesh = renderable.mesh;
            var shader = shadowmapPass ? renderable.material.shadowmapShader : renderable.material.shader;
            this.pipeline = shader.GetPipeline(renderable.mesh.vertexFormat);
            this.transformResourceSet = renderable.transformResourceSet;
            this.materialResourceSet = renderable.materialResourceSet;
            this.instanceResourceSet = renderable.instanceResourceSet;
            this.skeletonResourceSet = renderable.skeletonResourceSet;
        }
    }
}