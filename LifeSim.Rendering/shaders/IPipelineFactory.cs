namespace LifeSim.Engine.Rendering
{
    public interface IPass
    {
        Veldrid.Pipeline MakePipeline(ShaderVariant shaderVariant);
    }
}