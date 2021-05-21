using System;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class FullscreenPass : IDisposable, PipelineCache.IPipelineFactory
    {
        public ResourceSet _resourceSet;
        
        public ResourceLayout _resourceLayout;

        private FullScreenQuad _quad;

        private IRenderTexture _renderTexture;

        private GraphicsDevice _gd;

        public FullscreenPass(GraphicsDevice gd, IRenderTexture renderTexture)
        {
            this._gd = gd;
            var factory = gd.ResourceFactory;

            this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                Array.Empty<ResourceLayoutElementDescription>()
            ));

            this._resourceSet = factory.CreateResourceSet(new ResourceSetDescription(this._resourceLayout));

            this._quad = new FullScreenQuad(gd);
            this._renderTexture = renderTexture;
        }

        public void Render(CommandList cl, Material material)
        {
            cl.SetFramebuffer(this._renderTexture.framebuffer);
            var pipeline = material.shader.GetPipeline(this._quad.vertexFormat);
            cl.SetPipeline(pipeline);
            cl.SetVertexBuffer(0, this._quad.vertexBuffer);
            cl.SetGraphicsResourceSet(0, this._resourceSet);
            cl.SetGraphicsResourceSet(1, material.GetMaterialResourceSet());
            cl.Draw(6);
        }

        Pipeline PipelineCache.IPipelineFactory.MakePipeline(ShaderVariant shaderVariant)
        {
            var rasterizerState = new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: true
            );

            var resources = new Veldrid.ResourceLayout[] {
                this._resourceLayout,
                shaderVariant.materialResourceLayout,
            };

            return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription() {
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ShaderSet = shaderVariant.shaderSetDescription,
                BlendState = BlendStateDescription.SingleOverrideBlend,
                RasterizerState = rasterizerState,
                Outputs = this._renderTexture.outputDescription,
                ResourceLayouts = resources,
            });
        }

        public void Dispose()
        {
            this._resourceSet.Dispose();
            this._resourceLayout.Dispose();
        }
    }
}