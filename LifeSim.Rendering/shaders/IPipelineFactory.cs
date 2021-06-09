namespace LifeSim.Rendering
{
    public interface IPass
    {
        Veldrid.Pipeline MakePipeline(ShaderVariant shaderVariant);
    }
}