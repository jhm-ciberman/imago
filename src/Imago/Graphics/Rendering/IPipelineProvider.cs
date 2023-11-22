using Imago.Graphics.Materials;
using Veldrid;

namespace Imago.Graphics.Rendering;

public interface IPipelineProvider
{
    Pipeline MakePipeline(ShaderVariant shaderVariant, RenderFlags flags, TextureSampleCount sampleCount);
}
