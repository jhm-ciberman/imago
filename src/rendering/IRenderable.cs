using Veldrid;

namespace LifeSim.Rendering
{
    public interface IRenderable
    {
        VertexLayoutKind vertexLayoutKind { get; }
        
        ResourceLayout? resourceLayout { get; }

        string[] GetShaderKeywords();
    }
}