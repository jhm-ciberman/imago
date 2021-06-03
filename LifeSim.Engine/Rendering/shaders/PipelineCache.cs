using System.Collections.Generic;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Engine.Rendering
{
    public class PipelineCache
    {
        public interface IShaderVariantProvider
        {
            ShaderVariant GetShaderVariant(VertexFormat vertexFormat);
        }

        public interface IPipelineFactory
        {
            Veldrid.Pipeline MakePipeline(ShaderVariant shaderVariant);
        }

        private Veldrid.ResourceFactory _factory;

        private IPipelineFactory _pipelineFactory;

        private IShaderVariantProvider _shaderVariantProvider;

        private Veldrid.ResourceLayout _materialResourceLayout;

        public PipelineCache(
            Veldrid.ResourceFactory factory, 
            IPipelineFactory pipelineFactory,
            IShaderVariantProvider shaderVariantProvider,
            Veldrid.ResourceLayout materialResourceLayout
        )
        {
            this._factory = factory;
            this._pipelineFactory = pipelineFactory;
            this._shaderVariantProvider = shaderVariantProvider;
            this._materialResourceLayout = materialResourceLayout;
        }

        
        struct CachedPipeline
        {
            public VertexFormat vertexFormat;
            public Veldrid.Pipeline pipeline;

            public CachedPipeline(VertexFormat vertexFormat, Pipeline pipeline)
            {
                this.vertexFormat = vertexFormat;
                this.pipeline = pipeline;
            }
        }

        private readonly List<CachedPipeline> _pipelines = new List<CachedPipeline>();

        public Pipeline GetPipeline(VertexFormat vertexFormat)
        {
            lock (this._pipelines) {
                for (int i = 0; i < this._pipelines.Count; i++) {
                    if (this._pipelines[i].vertexFormat == vertexFormat) {
                        return this._pipelines[i].pipeline;
                    }
                }

                ShaderVariant shaderVariant = this._shaderVariantProvider.GetShaderVariant(vertexFormat);
                var pipeline = this._pipelineFactory.MakePipeline(shaderVariant);
                this._pipelines.Add(new CachedPipeline(vertexFormat, pipeline));
                return pipeline;
            }
        }
    }
}