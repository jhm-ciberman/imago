using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public interface IRenderingResourcesFactory
    {
        Mesh CreateMesh<T>(VertexFormat vertexFormat, T[] vertices, ushort[] indices, ref BoundingBox boundingBox) where T : unmanaged;
    }
}