using System;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Engine.Rendering
{
    public class VertexFormat
    {
        public VertexLayoutDescription[] layout;

        public MacroDefinition[] macroDefinitions;

        public VertexFormat(VertexLayoutDescription[] layout, MacroDefinition[]? macroDefinitions = null)
        {
            this.layout = layout;
            this.macroDefinitions = macroDefinitions ?? Array.Empty<MacroDefinition>();
        }
    }
}