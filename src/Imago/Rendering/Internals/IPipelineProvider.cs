using Imago.Assets.Materials;
using NeoVeldrid;

namespace Imago.Rendering.Internals;

internal interface IPipelineProvider
{
    public Pipeline MakePipeline(ShaderVariant shaderVariant, RenderFlags flags, TextureSampleCount sampleCount);
}
