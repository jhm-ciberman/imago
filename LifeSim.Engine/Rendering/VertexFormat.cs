using Veldrid;

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
}