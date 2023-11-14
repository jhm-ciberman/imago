using Imago.Rendering.Materials;
using Veldrid;

namespace Imago.Rendering;

public interface IPipelineProvider
{
    Pipeline MakePipeline(ShaderVariant shaderVariant, RenderFlags flags, TextureSampleCount sampleCount);
}
