namespace LifeSim.Engine.Rendering
{
    public interface IPipelineProvider
    {
        Veldrid.Pipeline MakePipeline(ShaderVariant shaderVariant);
    }
}