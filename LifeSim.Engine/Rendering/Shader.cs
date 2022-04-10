using System;
using System.Collections.Generic;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class Shader : IDisposable
{
    private static int _count = 0;

    public int Id { get; set; }
    private readonly List<CachedPipeline> _pipelines = new List<CachedPipeline>();
    public ResourceLayout MaterialResourceLayout { get; }
    private readonly List<ShaderVariant> _variants = new List<ShaderVariant>();
    private readonly GraphicsDevice _gd;
    public string VertexCode { get; }
    public string FragmentCode { get; }
    public string[] Textures { get; internal set; }

    public Shader(string vertexCode, string fragmentCode, string[]? textures = null)
    {
        this.Id = ++Shader._count;

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

    public Pipeline GetPipeline(IPipelineProvider pass, VertexFormat vertexFormat, RenderFlags flags)
    {
        lock (this._pipelines)
        {
            for (int i = 0; i < this._pipelines.Count; i++)
            {
                if (this._pipelines[i].VertexFormat == vertexFormat && this._pipelines[i].Flags == flags)
                    return this._pipelines[i].Pipeline;
            }

            ShaderVariant shaderVariant = this.GetShaderVariant(vertexFormat);
            var pipeline = pass.MakePipeline(shaderVariant, flags);
            this._pipelines.Add(new CachedPipeline(vertexFormat, pipeline, flags));

            return pipeline;
        }
    }

    private ShaderVariant GetShaderVariant(VertexFormat vertexFormat)
    {
        lock (this._variants)
        {
            for (int i = 0; i < this._variants.Count; i++)
            {
                if (this._variants[i].VertexFormat == vertexFormat)
                {
                    return this._variants[i];
                }
            }

            var macros = vertexFormat.GetMacroDefinitions();
            var shaders = ShaderCompiler.CompileShaders(this._gd, this.VertexCode, this.FragmentCode, macros);
            var variant = new ShaderVariant(this, vertexFormat, shaders);
            this._variants.Add(variant);
            return variant;
        }
    }

    public void Dispose()
    {
        this.MaterialResourceLayout.Dispose();
        foreach (var variant in this._variants)
        {
            variant.Dispose();
        }
        foreach (var pipeline in this._pipelines)
        {
            pipeline.Pipeline.Dispose();
        }
    }

    private struct CachedPipeline
    {
        public VertexFormat VertexFormat { get; }
        public Pipeline Pipeline { get; }
        public RenderFlags Flags { get; }

        public CachedPipeline(VertexFormat vertexFormat, Pipeline pipeline, RenderFlags flags)
        {
            this.VertexFormat = vertexFormat;
            this.Pipeline = pipeline;
            this.Flags = flags;
        }
    }
}

public class ShaderVariant : IDisposable
{
    public Shader Shader { get; }

    public VertexFormat VertexFormat { get; }

    public Veldrid.Shader[] Shaders { get; }

    public ShaderSetDescription ShaderSetDescription { get; internal set; }

    public ResourceLayout MaterialResourceLayout => this.Shader.MaterialResourceLayout;

    internal ShaderVariant(Shader shader, VertexFormat vertexFormat, Veldrid.Shader[] shaders)
    {
        this.Shader = shader;
        this.VertexFormat = vertexFormat;
        this.Shaders = shaders;
        this.ShaderSetDescription = new ShaderSetDescription(vertexFormat.Layouts, shaders);
    }

    public void Dispose()
    {
        foreach (var shader in this.Shaders)
        {
            shader.Dispose();
        }
    }
}