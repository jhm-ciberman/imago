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

        private Veldrid.VertexLayoutDescription _GetVertexLayout(VertexLayoutKind kind)
        {
            switch (kind)
            {
                case VertexLayoutKind.PosOnly:
                    return new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
                    );
                case VertexLayoutKind.Regular:
                    return new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
                    );
                case VertexLayoutKind.Skinned:
                    return new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                        new VertexElementDescription("Joints", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UShort4),
                        new VertexElementDescription("Weights", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
                    );
                case VertexLayoutKind.Sprite:
                    return new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                        new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)
                    );
                default:
                    throw new System.NotSupportedException();
            }
        }

        public ResourceLayout[] _GetResourceLayouts(Pass pass, IMaterial material, IRenderable renderable)
        {
            List<ResourceLayout> list = new List<ResourceLayout>(3);
            if (pass.resourceLayout != null)         list.Add(pass.resourceLayout);
            if (material.resourceLayout != null)     list.Add(material.resourceLayout);
            if (renderable.resourceLayout != null)   list.Add(renderable.resourceLayout);

            return list.ToArray();
        }

        private uint _GetHash(Pass pass, IMaterial material, IRenderable renderable)
        {
            uint kind = (uint) renderable.vertexLayoutKind;
            uint objectHash = kind;
            return (pass.id << 24) | objectHash;
        }

        public Pipeline GetPipeline(Pass pass, IMaterial material, IRenderable renderable)
        {
            uint hash = this._GetHash(pass, material, renderable);
            if (this._pipelines.TryGetValue(hash, out Pipeline? pipeline)) {
                return pipeline;
            }
            pipeline = this._MakePSO(pass, material, renderable);
            lock (this._pipelines) {
                this._pipelines.Add(hash, pipeline);
            }
            return pipeline;
        }

        private Pipeline _MakePSO(Pass pass, IMaterial material, IRenderable renderable)
        {
            var vertexLayout = this._GetVertexLayout(renderable.vertexLayoutKind);
            var shaders = this._shaderManager.GetShader(new ShaderVariant(pass.shaderName, renderable.GetShaderKeywords()));

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.ShaderSet = new ShaderSetDescription(new [] { vertexLayout }, shaders.shaders); 
            pipelineDescription.BlendState = pass.description.blendState;
            pipelineDescription.DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual;
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                pass.description.faceCullMode,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false
            );
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
            pipelineDescription.ResourceLayouts = this._GetResourceLayouts(pass, material, renderable);
            pipelineDescription.Outputs = pass.description.outputDescription;

            var pipeline = this._factory.CreateGraphicsPipeline(pipelineDescription);

            return pipeline;
        }
    }
}