using Imago.Rendering.Materials;

namespace Imago.Rendering;

public interface IPipelineProvider
{
    Veldrid.Pipeline MakePipeline(ShaderVariant shaderVariant, RenderFlags flags);
}
