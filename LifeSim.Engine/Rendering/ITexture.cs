using System;

namespace LifeSim.Engine.Rendering;

public interface ITexture : IDisposable
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

    /// <summary>
    /// Raised when the render texture is resized.
    /// </summary>
    event EventHandler? Resized;

    /// <summary>
    /// Resizes the render texture.
    /// </summary>
    /// <param name="width">The new width of the render texture.</param>
    /// <param name="height">The new height of the render texture.</param>
    void Resize(uint width, uint height);
}
