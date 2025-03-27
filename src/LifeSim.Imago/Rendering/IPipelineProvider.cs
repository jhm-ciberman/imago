using LifeSim.Imago.Materials;
using Veldrid;

namespace LifeSim.Imago.Rendering;

internal interface IPipelineProvider
{
    public Pipeline MakePipeline(ShaderVariant shaderVariant, RenderFlags flags, TextureSampleCount sampleCount);
}
