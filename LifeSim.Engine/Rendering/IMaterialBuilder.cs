using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public interface IMaterialBuilder
    {
        Pass colorPass { get; }
        Pass shadowMapPass { get; }
        Pass fullscreenPass { get; }
        Pass spritesPass { get; }
        
        ShaderLayouts layouts { get; }

        Sampler linearSampler { get; }
        Sampler pointSampler { get; }
        Texture pinkTexture { get; }

        ResourceSet CreateResourceSet(IMaterial material, params BindableResource[] resources);
    }
}