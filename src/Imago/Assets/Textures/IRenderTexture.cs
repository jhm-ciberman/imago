using System;
using Veldrid;

namespace Imago.Assets.Textures;

/// <summary>
/// Defines a contract for textures that can be used as rendering targets.
/// </summary>
public interface IRenderTexture : ITexture, IDisposable
{
    /// <summary>
    /// Gets the framebuffer of the render texture.
    /// </summary>
    public Framebuffer Framebuffer { get; }

    /// <summary>
    /// Gets the OutputDescription of the render texture.
    /// </summary>
    public OutputDescription OutputDescription { get; }
}
