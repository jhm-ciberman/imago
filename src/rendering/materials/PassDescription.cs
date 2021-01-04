using Veldrid;

namespace LifeSim.Rendering
{
    public struct PassDescription
    {
        public Shader shader;
        public OutputDescription outputDescription; 
        public FaceCullMode faceCullMode;
        public PolygonFillMode polygonFillMode;
        public BlendStateDescription blendState;
        public BindableResource[] resources;
        public DepthStencilStateDescription depthStencilState;

        public PassDescription(
            Shader shader, 
            OutputDescription outputDescription, 
            FaceCullMode faceCullMode, 
            PolygonFillMode polygonFillMode, 
            BlendStateDescription blendState, 
            DepthStencilStateDescription depthStencilState,
            BindableResource[] resources
        )
        {
            this.shader = shader;
            this.outputDescription = outputDescription;
            this.faceCullMode = faceCullMode;
            this.polygonFillMode = polygonFillMode;
            this.blendState = blendState;
            this.depthStencilState = depthStencilState;
            this.resources = resources;
        }
    }
}