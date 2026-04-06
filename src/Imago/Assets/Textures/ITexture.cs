using System;

namespace Imago.Assets.Textures;

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
    /// Gets the underlying NeoVeldrid texture.
    /// </summary>
    public NeoVeldrid.Texture NativeTexture { get; }

    /// <summary>
    /// Gets the underlying NeoVeldrid sampler.
    /// </summary>
    public NeoVeldrid.Sampler NativeSampler { get; }

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
