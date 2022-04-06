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

    internal ShaderVariant(GraphicsDevice gd, VertexFormat vertexFormat, ResourceLayout? materialResourceLayout, string vertexCode, string fragmentCode)
    {
        this.VertexFormat = vertexFormat;
        this.MaterialResourceLayout = materialResourceLayout;
        var layouts = GetVertexLayout(vertexFormat.Layouts, vertexFormat.IsSurface);
        var macros = GetMacroDefinitions(vertexFormat.Layouts);
        this.ShaderSetDescription = ShaderCompiler.Compile(gd, layouts, vertexCode, fragmentCode, macros);
    }

    private static List<MacroDefinition> GetMacroDefinitions(VertexLayoutDescription[] vertexLayouts)
    {
        var macros = new List<MacroDefinition>();
        foreach (var layout in vertexLayouts)
        {
            foreach (var element in layout.Elements)
            {
                macros.Add(new MacroDefinition("USE_" + element.Name.ToUpperInvariant()));
            }
        }
        return macros;
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