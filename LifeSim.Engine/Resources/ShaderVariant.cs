using System;
using System.Collections.Generic;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Engine.Rendering;

public class ShaderVariant : IDisposable
{
    public VertexFormat VertexFormat { get; }

    public ShaderSetDescription ShaderSetDescription { get; }

    public ResourceLayout? MaterialResourceLayout { get; }

    internal ShaderVariant(GraphicsDevice gd, VertexFormat vertexFormat, Shader shader)
    {
        this.VertexFormat = vertexFormat;
        this.MaterialResourceLayout = shader.MaterialResourceLayout;
        var layouts = GetVertexLayout(vertexFormat.Layouts, vertexFormat.IsSurface);
        var macros = vertexFormat.GetMacroDefinitions();
        var shaders = ShaderCompiler.CompileShaders(gd, shader.VertexCode, shader.FragmentCode, macros);
        this.ShaderSetDescription = new ShaderSetDescription(layouts, shaders);
    }

    private static VertexLayoutDescription[] GetVertexLayout(VertexLayoutDescription[] vertexLayouts, bool isSurface)
    {
        if (!isSurface)
        {
            return vertexLayouts;
        }

        var list = new List<VertexLayoutDescription>(vertexLayouts.Length + 1)
        {
            new VertexLayoutDescription(stride: 16, instanceStepRate: 1, new VertexElementDescription("Offsets", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt4))
        };
        list.AddRange(vertexLayouts);
        return list.ToArray();
    }

    public void Dispose()
    {
        var nativeShaders = this.ShaderSetDescription.Shaders;
        for (int i = 0; i < nativeShaders.Length; i++)
        {
            nativeShaders[i].Dispose();
        }
    }
}