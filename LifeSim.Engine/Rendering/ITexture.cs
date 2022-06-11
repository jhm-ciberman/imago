namespace LifeSim.Engine.Rendering;

public interface ITexture
{
    /// <summary>
    /// Gets the width of the texture.
    /// </summary>
    uint Width { get; }

    /// <summary>
    /// Gets the height of the texture.
    /// </summary>
    uint Height { get; }

    /// <summary>
    /// Gets the underlying Veldrid texture.
    /// </summary>
    Veldrid.Texture VeldridTexture { get; }

    /// <summary>
    /// Gets the underlying Veldrid sampler.
    /// </summary>
    Veldrid.Sampler VeldridSampler { get; }
}