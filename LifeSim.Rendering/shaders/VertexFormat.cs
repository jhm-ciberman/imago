using System;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Rendering
{
    public class VertexFormat
    {
        public VertexLayoutDescription Layout;
        public bool IsSkinned = false;
        public bool IsSurface = false;

        public VertexFormat(VertexLayoutDescription layout)
        {
            this.Layout = layout;
        }
    }
}