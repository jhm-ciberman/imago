using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public interface IGeometry
    {
        VertexFormat vertexFormat { get; }
        Veldrid.DeviceBuffer vertexBuffer { get; }
        Veldrid.DeviceBuffer indexBuffer { get; }
    }
}