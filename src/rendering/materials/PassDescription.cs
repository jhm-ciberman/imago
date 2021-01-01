using Veldrid;

namespace LifeSim.Rendering
{
    public struct PassDescription
    {
        public Shader shader;
        public IRenderTexture renderTexture; 
        public FaceCullMode faceCullMode;
        public PolygonFillMode polygonFillMode;
        public BlendStateDescription blendState;
        public BindableResource[] resources;

        public PassDescription(Shader shader, IRenderTexture renderTexture, FaceCullMode faceCullMode, PolygonFillMode polygonFillMode, BlendStateDescription blendState, BindableResource[] resources)
        {
            this.shader = shader;
            this.renderTexture = renderTexture;
            this.faceCullMode = faceCullMode;
            this.polygonFillMode = polygonFillMode;
            this.blendState = blendState;
            this.resources = resources;
        }
    }
}