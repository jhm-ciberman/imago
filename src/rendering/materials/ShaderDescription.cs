using Veldrid;

namespace LifeSim.Rendering
{
    public struct ShaderDescription
    {
        public string filename;
        public ResourceLayout[] resourcelayouts;
        public VertexLayoutDescription[] vertexLayouts;

        public ShaderDescription(string filename, ResourceLayout[] resourcelayouts, VertexLayoutDescription[] vertexLayouts)
        {
            this.filename = filename;
            this.resourcelayouts = resourcelayouts;
            this.vertexLayouts = vertexLayouts;
        }
    }
}