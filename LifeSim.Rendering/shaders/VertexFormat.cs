using System;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Engine.Rendering
{
    public class VertexFormat
    {
        public VertexLayoutDescription[] layout;

        public MacroDefinition[] macroDefinitions;

        public bool isSkinned = false;

        public VertexFormat(VertexLayoutDescription layout, MacroDefinition[]? macroDefinitions = null)
            : this(new VertexLayoutDescription[] { layout }, macroDefinitions)
        {
            //
        }

        public VertexFormat(bool isSkinned, VertexLayoutDescription[] layout, MacroDefinition[]? macroDefinitions = null)
            : this(layout, macroDefinitions)
        {
            this.isSkinned = isSkinned;
        }

        public VertexFormat(VertexLayoutDescription[] layout, MacroDefinition[]? macroDefinitions = null)
        {
            this.layout = layout;
            this.macroDefinitions = macroDefinitions ?? Array.Empty<MacroDefinition>();
        }

        public static VertexFormat CreateSurfaceVertexFormat(bool isSkinned, VertexLayoutDescription layout, MacroDefinition[]? macroDefinitions = null)
        {
            return new VertexFormat(isSkinned, new VertexLayoutDescription[] { new VertexLayoutDescription(stride: 16, instanceStepRate: 1,
                new VertexElementDescription("Offsets", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt4)
            ), layout}, macroDefinitions);
        }
    }
}