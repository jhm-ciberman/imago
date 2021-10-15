using System;
using System.Collections.Generic;
using Veldrid;

namespace LifeSim.Rendering
{
    public class Shader : IDisposable
    {
        private static int _count = 0;
        public int Id;
        private readonly List<CachedPipeline> _pipelines = new List<CachedPipeline>();
        private readonly ResourceLayout? _materialResourceLayout;
        private readonly List<ShaderVariant> _variants = new List<ShaderVariant>();
        private readonly ResourceFactory _factory;
        private readonly ShaderSource _source;

        public IPipelineProvider Pass { get; private set; }

        internal Shader(IPipelineProvider pass, ShaderSource source, ResourceLayout? materialResourceLayout = null)
        {
            this.Id = ++Shader._count;

            this.Pass = pass;

            this._source = source;

            this._factory = Renderer.GraphicsDevice.ResourceFactory;

            this._materialResourceLayout = materialResourceLayout;
        }

        public ResourceSet CreateResourceSet(params BindableResource[] resources)
        {
            return this._factory.CreateResourceSet(new ResourceSetDescription(this._materialResourceLayout, resources));
        }

        public Pipeline GetPipeline(VertexFormat vertexFormat)
        {
            for (int i = 0; i < this._pipelines.Count; i++)
            {
                if (this._pipelines[i].VertexFormat == vertexFormat)
                    return this._pipelines[i].Pipeline;
            }

            lock (this._pipelines)
            {
                // Search again, but this time with locking
                for (int i = 0; i < this._pipelines.Count; i++)
                {
                    if (this._pipelines[i].VertexFormat == vertexFormat)
                        return this._pipelines[i].Pipeline;
                }

                ShaderVariant shaderVariant = this._GetShaderVariant(vertexFormat);
                var pipeline = this.Pass.MakePipeline(shaderVariant);
                this._pipelines.Add(new CachedPipeline(vertexFormat, pipeline));
                return pipeline;
            }
        }

        private ShaderVariant _GetShaderVariant(VertexFormat vertexFormat)
        {
            for (int i = 0; i < this._variants.Count; i++)
            {
                if (this._variants[i].VertexFormat == vertexFormat)
                {
                    return this._variants[i];
                }
            }

            var variant = new ShaderVariant(this._factory, vertexFormat, this._materialResourceLayout, this._source);
            this._variants.Add(variant);
            return variant;
        }

        public void Dispose()
        {
            for (int i = 0; i < this._variants.Count; i++)
            {
                this._variants[i].Dispose();
            }
            this._materialResourceLayout?.Dispose();
        }

        private struct CachedPipeline
        {
            public VertexFormat VertexFormat;
            public Pipeline Pipeline;

            public CachedPipeline(VertexFormat vertexFormat, Pipeline pipeline)
            {
                this.VertexFormat = vertexFormat;
                this.Pipeline = pipeline;
            }
        }
    }
}