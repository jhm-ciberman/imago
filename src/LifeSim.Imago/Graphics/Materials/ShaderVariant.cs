using System;
using LifeSim.Imago.Graphics.Meshes;
using Veldrid;

namespace LifeSim.Imago.Graphics.Materials;

internal class ShaderVariant : IDisposable
{
    public Shader Shader { get; }

    public VertexFormat VertexFormat { get; }

    public Veldrid.Shader[] VeldridShaders { get; }

    public ShaderSetDescription ShaderSetDescription { get; internal set; }

    public ResourceLayout MaterialResourceLayout => this.Shader.MaterialResourceLayout;

    public RenderFlags Flags { get; }

    internal ShaderVariant(Shader shader, VertexFormat vertexFormat, Veldrid.Shader[] shaders, RenderFlags flags)
    {
        this.Shader = shader;
        this.VertexFormat = vertexFormat;
        this.VeldridShaders = shaders;
        this.Flags = flags;
        this.ShaderSetDescription = new ShaderSetDescription(vertexFormat.Layouts, shaders);
    }

    public void Dispose()
    {
        foreach (var shader in this.VeldridShaders)
        {
            shader.Dispose();
        }
    }
}
