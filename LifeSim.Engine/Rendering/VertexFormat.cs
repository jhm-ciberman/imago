using System;
using System.Collections.Generic;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Engine.Rendering;

public class VertexFormat
{
    public VertexLayoutDescription[] Layouts { get; set; }
    public bool IsSkinned { get; set; } = false;
    public bool IsSurface { get; set; } = false;

    public VertexFormat(params VertexLayoutDescription[] layouts)
    {
        this.Layouts = layouts;
    }

    public List<MacroDefinition> GetMacroDefinitions()
    {
        var macros = new List<MacroDefinition>();
        foreach (var layout in this.Layouts)
        {
            foreach (var element in layout.Elements)
            {
                macros.Add(new MacroDefinition("USE_" + element.Name.ToUpperInvariant()));
            }
        }
        return macros;
    }
}