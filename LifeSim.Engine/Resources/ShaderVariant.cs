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
        var layout = GetVertexLayout(vertexFormat);
        var macros = GetMacroDefinitions(vertexFormat.Layouts);
        var shaderDescription = ShaderCompiler.Compile(gd, vertexCode, fragmentCode, macros);
        this.ShaderSetDescription = new ShaderSetDescription(layout, shaderDescription);
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

    private static VertexLayoutDescription[] GetVertexLayout(VertexFormat vertexFormat)
    {
        if (!vertexFormat.IsSurface)
            return vertexFormat.Layouts;

        var arr = new VertexLayoutDescription[vertexFormat.Layouts.Length + 1];
        arr[0] = new VertexLayoutDescription(stride: 16, instanceStepRate: 1,
            new VertexElementDescription("Offsets", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt4)
        );
        Array.Copy(vertexFormat.Layouts, 0, arr, 1, vertexFormat.Layouts.Length);
        return arr;
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