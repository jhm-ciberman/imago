using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public interface IMaterialBuilder
    {
        ResourceLayouts layouts { get; }
        PassManager passes { get; }

        Sampler linearSampler { get; }
        Sampler pointSampler { get; }
        GPUTexture pinkTexture { get; }

        ResourceSet CreateResourceSet(IMaterial material, params BindableResource[] resources);
    }
}