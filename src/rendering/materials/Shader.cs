using Veldrid;

namespace LifeSim.Rendering
{
    public class Shader
    {
        public readonly ShaderSetDescription shaderSet; 
        public readonly ResourceLayout[] resourceLayouts; 

        public Shader(ShaderSetDescription shaderSet, ResourceLayout[] resourceLayouts)
        {
            this.shaderSet = shaderSet;
            this.resourceLayouts = resourceLayouts;
        }
    }
}