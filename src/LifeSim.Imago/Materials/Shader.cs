using System;
using System.Collections.Generic;
using LifeSim.Imago.Rendering;
using LifeSim.Imago.Meshes;
using Veldrid;

namespace LifeSim.Imago.Materials;

/// <summary>
/// Represents a shader used for rendering.
/// </summary>
public class Shader : IDisposable
{
    private record struct CachedPipeline(VertexFormat VertexFormat, Pipeline Pipeline, RenderFlags Flags, TextureSampleCount SampleCount);

    /// <summary>
    /// Gets an array of the names of the textures used by this shader.
    /// </summary>
    internal string[] Textures { get; }

    /// <summary>
    /// Gets the resource layout for the material.
    /// </summary>
    internal ResourceLayout MaterialResourceLayout { get; }

    private readonly List<CachedPipeline> _pipelines = [];

    private readonly List<ShaderVariant> _variants = [];

    private readonly GraphicsDevice _gd;

    private readonly IPipelineProvider _pass;

    private readonly RenderFlags _usedRenderFlags = RenderFlags.None;

    private readonly string _vertexCode;

    private readonly string _fragmentCode;

    private readonly Renderer _renderer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Shader"/> class.
    /// </summary>
    /// <param name="renderer">The renderer.</param>
    /// <param name="pass">The pass to use.</param>
    /// <param name="vertexCode">The source vertex shader code.</param>
    /// <param name="fragmentCode">The source fragment shader code.</param>
    /// <param name="textures">The names of the textures used by this shader.</param>
    internal Shader(Renderer renderer, IPipelineProvider pass, string vertexCode, string fragmentCode, string[]? textures = null)
    {
        this._renderer = renderer;
        this._pass = pass;
        this._vertexCode = vertexCode;
        this._fragmentCode = fragmentCode;
        this._usedRenderFlags = InferUsedRenderFlags(vertexCode) | InferUsedRenderFlags(fragmentCode);

        this._gd = this._renderer.GraphicsDevice;

        this.Textures = textures ?? [];
        this.MaterialResourceLayout = this._renderer.GetResourceLayout(this.MakeResourceLayoutDescription());
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

    /// <summary>
    /// Gets or creates a veldrid pipeline.
    /// </summary>
    /// <param name="vertexFormat">The vertex format to use.</param>
    /// <param name="flags">The render flags to use.</param>
    /// <param name="sampleCount">The sample count to use.</param>
    /// <returns>The veldrid pipeline.</returns>
    internal Pipeline GetPipeline(VertexFormat vertexFormat, RenderFlags flags, TextureSampleCount sampleCount = TextureSampleCount.Count1)
    {
        for (int i = 0; i < this._pipelines.Count; i++)
        {
            if (this._pipelines[i].VertexFormat == vertexFormat
            && this._pipelines[i].Flags == flags
            && this._pipelines[i].SampleCount == sampleCount)
            {
                return this._pipelines[i].Pipeline;
            }
        }

        ShaderVariant shaderVariant = this.GetShaderVariant(vertexFormat, flags);
        var pipeline = this._pass.MakePipeline(shaderVariant, flags, sampleCount);
        this._pipelines.Add(new CachedPipeline(vertexFormat, pipeline, flags, sampleCount));

        return pipeline;
    }

    private ShaderVariant GetShaderVariant(VertexFormat vertexFormat, RenderFlags flags)
    {
        flags &= this._usedRenderFlags;

        for (int i = 0; i < this._variants.Count; i++)
        {
            if (this._variants[i].VertexFormat == vertexFormat && this._variants[i].Flags == flags)
                return this._variants[i];
        }

        var macros = vertexFormat.GetMacroDefinitions();
        AddFlagsMacros(macros, flags);
        var shaders = ShaderCompiler.CompileShaders(this._gd, this._vertexCode, this._fragmentCode, macros);
        var variant = new ShaderVariant(this, vertexFormat, shaders, flags);
        this._variants.Add(variant);
        return variant;
    }

    private static readonly Dictionary<RenderFlags, string> _renderFlagMacros = new Dictionary<RenderFlags, string>
    {
        [RenderFlags.DoubleSided] = "ENABLE_DOUBLE_SIDED",
        [RenderFlags.Wireframe] = "ENABLE_WIREFRAME",
        [RenderFlags.Transparent] = "ENABLE_TRANSPARENT",
        [RenderFlags.AlphaTest] = "ENABLE_ALPHA_TEST",
        [RenderFlags.DepthTest] = "ENABLE_DEPTH_TEST",
        [RenderFlags.DepthWrite] = "ENABLE_DEPTH_WRITE",
        [RenderFlags.ReceiveShadows] = "ENABLE_RECEIVE_SHADOWS",
        [RenderFlags.Fog] = "ENABLE_FOG",
        [RenderFlags.PixelPerfactShadows] = "ENABLE_PIXEL_PERFECT_SHADOWS",
        [RenderFlags.ColorWrite] = "ENABLE_COLOR_WRITE",
        [RenderFlags.ShadowCascades] = "ENABLE_SHADOW_CASCADES",
        [RenderFlags.HalfLambert] = "ENABLE_HALF_LAMBERT",
    };

    private static void AddFlagsMacros(List<Veldrid.SPIRV.MacroDefinition> macros, RenderFlags flags)
    {
        foreach (var flag in _renderFlagMacros)
        {
            if (flags.HasFlag(flag.Key))
                macros.Add(new Veldrid.SPIRV.MacroDefinition(flag.Value, "1"));
        }
    }

    private static RenderFlags InferUsedRenderFlags(string code)
    {
        var flags = RenderFlags.None;
        foreach (var flag in _renderFlagMacros)
        {
            if (code.Contains(flag.Value))
                flags |= flag.Key;
        }

        return flags;
    }

    /// <summary>
    /// Disposes this shader.
    /// </summary>
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
}
