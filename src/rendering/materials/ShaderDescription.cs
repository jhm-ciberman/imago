using Veldrid;

namespace LifeSim.Rendering
{
    public struct ShaderDescription
    {
        public string filename;
        public ResourceLayout? passResourcelayout;
        public ResourceLayout? materialResourcelayout;
        public ResourceLayout? objectResourcelayout;
        public VertexLayoutDescription[] vertexLayouts;

        public ShaderDescription(
            string filename, 
            ResourceLayout passResourcelayout, 
            ResourceLayout materialResourcelayout, 
            ResourceLayout objectResourcelayout, 
            VertexLayoutDescription[] vertexLayouts
        )
        {
            this.filename = filename;
            this.passResourcelayout = passResourcelayout;
            this.materialResourcelayout = materialResourcelayout;
            this.objectResourcelayout = objectResourcelayout;
            this.vertexLayouts = vertexLayouts;
        }
    }
}