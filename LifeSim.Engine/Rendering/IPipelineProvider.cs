namespace LifeSim.Engine.Rendering;

internal interface IPipelineProvider
{
    Veldrid.Pipeline MakePipeline(ShaderVariant shaderVariant, RenderFlags flags);
}