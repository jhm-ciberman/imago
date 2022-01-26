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
    public ResourceLayout? MaterialResourceLayout { get; }
    private readonly List<ShaderVariant> _variants = new List<ShaderVariant>();
    private readonly GraphicsDevice _gd;
    private readonly string _vertexCode;
    private readonly string _fragmentCode;


    public Shader(Renderer renderer, IPipelineProvider pass, string vertexCode, string fragmentCode, ResourceLayout? materialResourceLayout = null)
    {
        this.Id = ++Shader._count;

        this.Pass = pass;

        this._vertexCode = vertexCode;
        this._fragmentCode = fragmentCode;

        this._gd = renderer.GraphicsDevice;

        this.MaterialResourceLayout = materialResourceLayout;
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

            ShaderVariant shaderVariant = this.GetShaderVariant(vertexFormat);
            var pipeline = this.Pass.MakePipeline(shaderVariant);
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

        var variant = new ShaderVariant(this._gd, vertexFormat, this.MaterialResourceLayout, this._vertexCode, this._fragmentCode);
        this._variants.Add(variant);
        return variant;
    }

    public void Dispose()
    {
        for (int i = 0; i < this._variants.Count; i++)
        {
            this._variants[i].Dispose();
        }
        this.MaterialResourceLayout?.Dispose();
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