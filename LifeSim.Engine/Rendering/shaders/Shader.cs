using System;
using System.Collections.Generic;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class Shader : IDisposable, PipelineCache.IShaderVariantProvider
    {
        private static int _count = 0;

        public enum UniformType
        {
            Vec4, 
            UVec4,
        }

        public struct Uniform
        {
            public string name;
            public UniformType type;

            public Uniform(string name, UniformType type)
            {
                this.name = name;
                this.type = type;
            }
        }

        /*

        public struct Texture
        {
            public string name;
            public uint textureBind;
            public uint samplerBind;

            public Texture(string name, uint textureBind, uint samplerBind)
            {
                this.name = name;
                this.textureBind = textureBind;
                this.samplerBind = samplerBind;
            }
        }
        */

        public int id;

        private ResourceLayout _materialResourceLayout;

        public IReadOnlyDictionary<string, uint> instanceUniformData;

        private List<ShaderVariant> _variants = new List<ShaderVariant>();

        private Veldrid.ResourceFactory _factory;

        internal Veldrid.GraphicsDevice _gd;

        private ShaderSource _source;

        public int vertexLayoutsFlags;

        private PipelineCache _pipelineCache;

        private ResourceLayoutElementDescription[] _resources;

        public Shader(
            Veldrid.GraphicsDevice gd,
            PipelineCache.IPipelineFactory pass,
            VertexFormat[] vertexFormats,
            ShaderSource source, 
            ResourceLayoutDescription materialResourceLayout,
            //Texture[] textures,
            Uniform[] uniforms
        )
        {
            this.id = ++Shader._count;
            this._source = source;

            this._gd = gd;
            this._factory = gd.ResourceFactory;
            this._materialResourceLayout = this._factory.CreateResourceLayout(materialResourceLayout);
            this._resources = materialResourceLayout.Elements;
            this._pipelineCache = new PipelineCache(this._factory, pass, this, this._materialResourceLayout);

            var dict = new Dictionary<string, uint>();
            for (int i = 0; i < uniforms.Length; i++) {
                dict.Add(uniforms[i].name, (uint) i);
            }
            this.instanceUniformData = dict;
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

        public int resourceCount => this._resources.Length;

        public Veldrid.ResourceKind GetResourceKind(int index)
        {
            return this._resources[index].Kind;
        }

        public string GetResourceName(int index)
        {
            return this._resources[index].Name;
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