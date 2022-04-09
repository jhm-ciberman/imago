using System;
using System.Collections.Generic;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class Shader : IDisposable
{
    private static int _count = 0;

    public int Id { get; set; }

    public IPipelineProvider Pass { get; private set; }

    private readonly List<CachedPipeline> _pipelines = new List<CachedPipeline>();
    public ResourceLayout MaterialResourceLayout { get; }
    private readonly List<ShaderVariant> _variants = new List<ShaderVariant>();
    private readonly GraphicsDevice _gd;
    public string VertexCode { get; }
    public string FragmentCode { get; }
    public string[] Textures { get; internal set; }

    public Shader(IPipelineProvider pass, string vertexCode, string fragmentCode, string[]? textures = null)
    {
        this.Id = ++Shader._count;

        this.Pass = pass;

        this.VertexCode = vertexCode;
        this.FragmentCode = fragmentCode;

        this._gd = Renderer.Instance.GraphicsDevice;

        this.Textures = textures ?? Array.Empty<string>();
        this.MaterialResourceLayout = Renderer.Instance.GetResourceLayout(this.MakeResourceLayoutDescription());
    }

    private ResourceLayoutDescription MakeResourceLayoutDescription()
    {
        var elements = new List<ResourceLayoutElementDescription>();
        for (int i = 0; i < this.Textures.Length; i++)
        {
            var name = this.Textures[i];
            elements.Add(new ResourceLayoutElementDescription(name + "Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment));
            elements.Add(new ResourceLayoutElementDescription(name + "Sampler", ResourceKind.Sampler, ShaderStages.Fragment));
        }

        return new ResourceLayoutDescription(elements.ToArray());
    }

    public Pipeline GetPipeline(IPipelineProvider pass, VertexFormat vertexFormat)
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

            ShaderVariant shaderVariant = this.GetShaderVariant(vertexFormat);
            var pipeline = pass.MakePipeline(shaderVariant);
            this._pipelines.Add(new CachedPipeline(vertexFormat, pipeline));

            return pipeline;
        }
    }

    private ShaderVariant GetShaderVariant(VertexFormat vertexFormat)
    {
        for (int i = 0; i < this._variants.Count; i++)
        {
            if (this._variants[i].VertexFormat == vertexFormat)
            {
                return this._variants[i];
            }
        }

        var variant = new ShaderVariant(this._gd, vertexFormat, this);
        this._variants.Add(variant);
        return variant;
    }

    public void Dispose()
    {
        for (int i = 0; i < this._variants.Count; i++)
        {
            this._variants[i].Dispose();
        }
        this.MaterialResourceLayout.Dispose();
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