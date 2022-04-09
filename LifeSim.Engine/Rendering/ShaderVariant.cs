using System;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class ShaderVariant : IDisposable
{
    public VertexFormat VertexFormat { get; }

    public ShaderSetDescription ShaderSetDescription { get; }

    public ResourceLayout MaterialResourceLayout { get; }

    public Veldrid.Shader[] Shaders { get; }

    internal ShaderVariant(GraphicsDevice gd, VertexFormat vertexFormat, Shader shader)
    {
        this.VertexFormat = vertexFormat;
        this.MaterialResourceLayout = shader.MaterialResourceLayout;
        var macros = vertexFormat.GetMacroDefinitions();
        this.Shaders = ShaderCompiler.CompileShaders(gd, shader.VertexCode, shader.FragmentCode, macros);
    }

    public void Dispose()
    {
        foreach (var shader in this.Shaders)
        {
            shader.Dispose();
        }
    }
}