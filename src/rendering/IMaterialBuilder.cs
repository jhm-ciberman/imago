using Veldrid;

namespace LifeSim.Rendering
{
    public interface IMaterialBuilder
    {
        ResourceLayouts layouts { get; }
        PassManager passes { get; }

        Sampler linearSampler { get; }
        Sampler pointSampler { get; }

        ResourceSet CreateResourceSet(IMaterial material, params BindableResource[] resources);
    }
}