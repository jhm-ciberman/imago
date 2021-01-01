using Veldrid;

namespace LifeSim.Rendering
{
    public struct PassDescription
    {
        public Shader shader;
        public IRenderTexture renderTexture; 
        public PolygonFillMode polygonFillMode;
        public BlendStateDescription blendState;
        public BindableResource[] resources;

        public PassDescription(Shader shader, IRenderTexture renderTexture, PolygonFillMode polygonFillMode, BlendStateDescription blendState, BindableResource[] resources)
        {
            this.shader = shader;
            this.renderTexture = renderTexture;
            this.polygonFillMode = polygonFillMode;
            this.blendState = blendState;
            this.resources = resources;
        }
    }
}