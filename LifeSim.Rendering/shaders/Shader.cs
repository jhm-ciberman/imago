using System;
using System.Collections.Generic;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class Shader : IDisposable, PipelineCache.IShaderVariantProvider
    {
        private static int _count = 0;
        public int id;

        private ResourceLayout _materialResourceLayout;

        private List<ShaderVariant> _variants = new List<ShaderVariant>();

        private Veldrid.ResourceFactory _factory;

        private ShaderSource _source;

        private PipelineCache _pipelineCache;
        public IPass pass { get; private set; }

        internal Shader(IPass pass, ShaderSource source, Veldrid.ResourceLayout materialResourceLayout)
        {
            this.id = ++Shader._count;

            this.pass = pass;

            this._source = source;

            this._factory = Renderer.graphicsDevice.ResourceFactory;

            this._materialResourceLayout = materialResourceLayout;
            this._pipelineCache = new PipelineCache(this._factory, pass, this);
        }

        public Pipeline GetPipeline(VertexFormat vertexFormat)
        {
            return this._pipelineCache.GetPipeline(vertexFormat);
        }

        public void Dispose()
        {
            for (int i = 0; i < this._variants.Count; i++) {
                this._variants[i].Dispose();
            }
            this._materialResourceLayout.Dispose();
        }



        public Veldrid.ResourceSet CreateResourceSet(params BindableResource[] resources)
        {
            return this._factory.CreateResourceSet(new ResourceSetDescription(this._materialResourceLayout, resources));
        }

        ShaderVariant PipelineCache.IShaderVariantProvider.GetShaderVariant(VertexFormat vertexFormat)
        {
            for (int i = 0; i < this._variants.Count; i++) {
                if (this._variants[i].vertexFormat == vertexFormat) {
                    return this._variants[i];
                }
            }

            var variant = new ShaderVariant(this._factory, vertexFormat, this._materialResourceLayout, this._source);
            this._variants.Add(variant);
            return variant;
        }


    }
}