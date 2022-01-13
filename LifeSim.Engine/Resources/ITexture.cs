namespace LifeSim.Engine.Rendering;

public interface ITexture
{
    uint Width { get; }
    uint Height { get; }
    Veldrid.Texture DeviceTexture { get; }
    Veldrid.Sampler Sampler { get; }
}