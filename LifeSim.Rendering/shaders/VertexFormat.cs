using System;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Rendering
{
    public class VertexFormat
    {
        public VertexLayoutDescription layout;
        public bool isSkinned = false;
        public bool isSurface = false;

        public VertexFormat(VertexLayoutDescription layout)
        {
            this.layout = layout;
        }
    }
}