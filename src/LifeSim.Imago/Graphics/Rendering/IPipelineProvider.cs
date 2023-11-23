using LifeSim.Imago.Graphics.Materials;
using Veldrid;

namespace LifeSim.Imago.Graphics.Rendering;

public interface IPipelineProvider
{
    Pipeline MakePipeline(ShaderVariant shaderVariant, RenderFlags flags, TextureSampleCount sampleCount);
}
