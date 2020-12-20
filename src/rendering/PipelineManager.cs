using System.Collections.Generic;
using Veldrid;

namespace LifeSim.Rendering
{
    public class PipelineManager
    {
        private ResourceLayout _cameraInfoLayout;
        public ResourceLayout cameraInfoLayout => this._cameraInfoLayout;

        private ResourceFactory _factory;
        private Dictionary<uint, GPUPipeline> _cache = new Dictionary<uint, GPUPipeline>(); // Key: MaterialPass.id

        private Framebuffer _framebuffer;

        public PipelineManager(ResourceFactory factory, Framebuffer framebuffer)
        {
            this._factory = factory;
            this._framebuffer = framebuffer;
            this._cameraInfoLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("CameraInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));
            Material.onRefCountZero += this._ReleaseMaterial;
        }

        private void _ReleaseMaterial(Material material)
        {
            if (this._cache.TryGetValue(material.pass.id, out GPUPipeline pipeline)) {
                pipeline.pipeline.Dispose();
                foreach (var layout in pipeline.resourceLayouts) {
                    layout.Dispose();
                }
                this._cache.Remove(material.pass.id);
                System.Console.WriteLine("Material Cleaned Up!");
            }
        }

        public class GPUPipeline
        {
            public Veldrid.Pipeline pipeline;
            public Veldrid.ResourceLayout[] resourceLayouts; 
        }

        public GPUPipeline GetPipeline(MaterialPass pass)
        {
            if (this._cache.TryGetValue(pass.id, out GPUPipeline pipeline)) {
                return pipeline;
            }

            pipeline = this._CreateNewPipeline(pass);
            this._cache.Add(pass.id, pipeline);
            return pipeline;
        }

        public GPUPipeline _CreateNewPipeline(MaterialPass pass)
        {
            var shader = pass.shader;
            var rasterizerState = new RasterizerStateDescription(
                FaceCullMode.Front,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false
            );

            var layoutDescriptions = shader.BuildResourceLayouts();
            var resourceLayouts = this._BuildResourceLayouts(layoutDescriptions);

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.ShaderSet = shader.BuildShaderSetDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual;
            pipelineDescription.RasterizerState = rasterizerState;
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
            pipelineDescription.ResourceLayouts = resourceLayouts;
            pipelineDescription.Outputs = this._framebuffer.OutputDescription; 

            return new GPUPipeline {
                pipeline = this._factory.CreateGraphicsPipeline(pipelineDescription),
                resourceLayouts = resourceLayouts,
            };
        }

        private ResourceLayout[] _BuildResourceLayouts(ResourceLayoutDescription[] descriptions)
        {
            ResourceLayout[] layouts = new ResourceLayout[descriptions.Length + 1];
            layouts[0] = this._cameraInfoLayout;
            int i = 1;
            foreach (var description in descriptions) {
                layouts[i++] = this._factory.CreateResourceLayout(description);
            }
            return layouts;
        }
    }
}