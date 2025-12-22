using LifeSim.Imago.Assets.Materials;
using Veldrid;

namespace LifeSim.Imago.Rendering.Internals;

internal interface IPipelineProvider
{
    public Pipeline MakePipeline(ShaderVariant shaderVariant, RenderFlags flags, TextureSampleCount sampleCount);
}
