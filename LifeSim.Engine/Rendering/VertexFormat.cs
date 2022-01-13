using Veldrid;

namespace LifeSim.Engine.Rendering;

public class VertexFormat
{
    public VertexLayoutDescription[] Layouts;
    public bool IsSkinned = false;
    public bool IsSurface = false;

    public VertexFormat(params VertexLayoutDescription[] layouts)
    {
        this.Layouts = layouts;
    }
}