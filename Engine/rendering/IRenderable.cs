using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public interface IRenderable
    {
        VertexLayoutKind vertexLayoutKind { get; }
        
        ResourceLayout? resourceLayout { get; }

        string[] GetShaderKeywords();
    }
}