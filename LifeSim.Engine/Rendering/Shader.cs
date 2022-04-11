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

    public RenderFlags SupportedRenderFlags { get; } = RenderFlags.None;

    public Shader(string vertexCode, string fragmentCode, string[]? textures = null, RenderFlags suportedRenderFlags = RenderFlags.None)
    {
        this.Id = ++Shader._count;

        this.VertexCode = vertexCode;
        this.FragmentCode = fragmentCode;
        this.SupportedRenderFlags = suportedRenderFlags;

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

            ShaderVariant shaderVariant = this.GetShaderVariant(vertexFormat, flags);
            var pipeline = pass.MakePipeline(shaderVariant, flags);
            this._pipelines.Add(new CachedPipeline(vertexFormat, pipeline, flags));

            return pipeline;
        }
    }

    private ShaderVariant GetShaderVariant(VertexFormat vertexFormat, RenderFlags flags)
    {
        flags &= this.SupportedRenderFlags;

        lock (this._variants)
        {
            for (int i = 0; i < this._variants.Count; i++)
            {
                if (this._variants[i].VertexFormat == vertexFormat && this._variants[i].Flags == flags)
                    return this._variants[i];
            }

            var macros = vertexFormat.GetMacroDefinitions();
            AddFlagsMacros(macros, flags);
            var shaders = ShaderCompiler.CompileShaders(this._gd, this.VertexCode, this.FragmentCode, macros);
            var variant = new ShaderVariant(this, vertexFormat, shaders, flags);
            this._variants.Add(variant);
            return variant;
        }
    }

    private static readonly Dictionary<RenderFlags, string> _renderFlagMacros = new Dictionary<RenderFlags, string>
    {
        [RenderFlags.DoubleSided] = "ENABLE_DOUBLE_SIDED",
        [RenderFlags.Wireframe] = "ENABLE_WIREFRAME",
        [RenderFlags.Transparent] = "ENABLE_ALPHA_BLENDING",
        [RenderFlags.AlphaTest] = "ENABLE_ALPHA_TEST",
        [RenderFlags.DepthTest] = "ENABLE_DEPTH_TEST",
        [RenderFlags.DepthWrite] = "ENABLE_DEPTH_WRITE",
        [RenderFlags.MousePick] = "ENABLE_MOUSE_PICK",
        [RenderFlags.ReceiveShadows] = "ENABLE_RECEIVE_SHADOWS",
        [RenderFlags.Fog] = "ENABLE_FOG",
    };

    private static void AddFlagsMacros(List<Veldrid.SPIRV.MacroDefinition> macros, RenderFlags flags)
    {
        foreach (var flag in _renderFlagMacros)
        {
            if (flags.HasFlag(flag.Key))
                macros.Add(new Veldrid.SPIRV.MacroDefinition(flag.Value, "1"));
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
        private static int _debugCounter = 0;
        public VertexFormat VertexFormat { get; }
        public Pipeline Pipeline { get; }
        public RenderFlags Flags { get; }

        public CachedPipeline(VertexFormat vertexFormat, Pipeline pipeline, RenderFlags flags)
        {
            this.VertexFormat = vertexFormat;
            this.Pipeline = pipeline;
            this.Flags = flags;

            // Debug:
            Console.WriteLine($"Cached pipeline {++_debugCounter} ({vertexFormat.Name} {flags})");
        }
    }
}

public class ShaderVariant : IDisposable
{
    private static int _debugCounter = 0;
    public Shader Shader { get; }

    public VertexFormat VertexFormat { get; }

    public Veldrid.Shader[] Shaders { get; }

    public ShaderSetDescription ShaderSetDescription { get; internal set; }

    public ResourceLayout MaterialResourceLayout => this.Shader.MaterialResourceLayout;

    public RenderFlags Flags { get; }

    internal ShaderVariant(Shader shader, VertexFormat vertexFormat, Veldrid.Shader[] shaders, RenderFlags flags)
    {
        this.Shader = shader;
        this.VertexFormat = vertexFormat;
        this.Shaders = shaders;
        this.Flags = flags;
        this.ShaderSetDescription = new ShaderSetDescription(vertexFormat.Layouts, shaders);

        // Debug:
        Console.WriteLine($"Shader variant {++_debugCounter} ({vertexFormat.Name} {flags})");
    }

    public void Dispose()
    {
        foreach (var shader in this.Shaders)
        {
            shader.Dispose();
        }
    }
}