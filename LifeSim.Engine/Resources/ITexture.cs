namespace LifeSim.Engine.Rendering
{
    public interface ITexture
    {
        int Width { get; }
        int Height { get; }
        Veldrid.Texture DeviceTexture { get; }
        Veldrid.Sampler Sampler { get; }
    }
}