using Imago.Graphics.Materials;
using Veldrid;

namespace Imago.Graphics;

public interface IPipelineProvider
{
    Pipeline MakePipeline(ShaderVariant shaderVariant, RenderFlags flags, TextureSampleCount sampleCount);
}
