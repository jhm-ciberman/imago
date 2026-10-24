using Imago.Graphics3D.Materials;
using NeoVeldrid;

namespace Imago.Graphics3D.Rendering.Internals;

internal interface IPipelineProvider
{
    public Pipeline MakePipeline(ShaderVariant shaderVariant, RenderFlags flags, TextureSampleCount sampleCount);
}
