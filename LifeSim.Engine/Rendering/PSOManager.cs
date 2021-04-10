using System.Collections.Generic;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class PSOManager
    {
        private readonly Veldrid.ResourceFactory _factory; 

        private readonly ShaderManager _shaderManager;

        private readonly Dictionary<uint, Pipeline> _pipelines = new Dictionary<uint, Pipeline>();

        public PSOManager(Veldrid.ResourceFactory factory)
        {
            this._factory = factory;
            this._shaderManager = new ShaderManager(factory);
        }

        public ResourceLayout[] _GetResourceLayouts(Pass pass, IMaterial material, IRenderable renderable)
        {
            List<ResourceLayout> list = new List<ResourceLayout>(3);
            list.Add(pass.resourceLayout);
            list.Add(material.resourceLayout);
            if (renderable.resourceLayout != null) {
                list.Add(renderable.resourceLayout);
            }

            return list.ToArray();
        }

        private uint _GetHash(Pass pass, IRenderable renderable)
        {
            uint kind = (uint) renderable.vertexLayoutKind;
            uint objectHash = kind;
            return (pass.id << 24) | objectHash;
        }

        public Pipeline GetPipeline(Pass pass, IMaterial material, IRenderable renderable)
        {
            uint hash = this._GetHash(pass, renderable);
            Pipeline? pipeline;
            lock (this._pipelines) {
                if (this._pipelines.TryGetValue(hash, out pipeline)) {
                    return pipeline;
                }
            }
            pipeline = this._MakePSO(pass, material, renderable);
            lock (this._pipelines) {
                this._pipelines.Add(hash, pipeline);
            }
            return pipeline;
        }

        private Pipeline _MakePSO(Pass pass, IMaterial material, IRenderable renderable)
        {
            var vertexLayout = ShaderLayouts.GetVertexLayout(renderable.vertexLayoutKind);
            var description = new ShaderVariantDescription(pass.shaderName, renderable.GetShaderKeywords());
            var shaderVariant = this._shaderManager.GetShaderVariant(description);

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.ShaderSet = new ShaderSetDescription(new [] { vertexLayout }, shaderVariant.shaders); 
            pipelineDescription.BlendState = pass.description.blendState;
            pipelineDescription.DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual;
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                pass.description.faceCullMode,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: true
            );
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
            pipelineDescription.ResourceLayouts = this._GetResourceLayouts(pass, material, renderable);
            pipelineDescription.Outputs = pass.description.outputDescription;

            var pipeline = this._factory.CreateGraphicsPipeline(pipelineDescription);

            return pipeline;
        }
    }
}