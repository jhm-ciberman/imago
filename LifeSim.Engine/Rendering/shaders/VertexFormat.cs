using System;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Engine.Rendering
{
    public class VertexFormat
    {
        public VertexLayoutDescription[] layout;

        public MacroDefinition[] macroDefinitions;

        public VertexFormat(bool isSurface, VertexLayoutDescription layout, MacroDefinition[]? macroDefinitions = null)
        {
            this.layout = isSurface 
                ? new VertexLayoutDescription[] { GetOffsetLayoutDescription(), layout } 
                : new VertexLayoutDescription[] { layout };

            this.macroDefinitions = macroDefinitions ?? Array.Empty<MacroDefinition>();
        }

        private static VertexLayoutDescription GetOffsetLayoutDescription()
        {
            return new VertexLayoutDescription(stride: 16, instanceStepRate: 1,
                new VertexElementDescription("Offsets", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt4)
            );
        }
    }
}