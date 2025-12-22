using System;

namespace LifeSim.Imago.Assets.Textures;

/// <summary>
/// Defines a contract for texture objects that can be used for rendering.
/// </summary>
public interface ITexture : IDisposable
{
    /// <summary>
    /// Gets the width of the texture.
    /// </summary>
    public uint Width { get; }

    /// <summary>
    /// Gets the height of the texture.
    /// </summary>
    public uint Height { get; }

    /// <summary>
    /// Gets the underlying Veldrid texture.
    /// </summary>
    public Veldrid.Texture VeldridTexture { get; }

    /// <summary>
    /// Gets the underlying Veldrid sampler.
    /// </summary>
    public Veldrid.Sampler VeldridSampler { get; }

    /// <summary>
    /// Raised when the render texture is resized.
    /// </summary>
    public event EventHandler? Resized;

    /// <summary>
    /// Resizes the render texture.
    /// </summary>
    /// <param name="width">The new width of the render texture.</param>
    /// <param name="height">The new height of the render texture.</param>
    public void Resize(uint width, uint height);
}
