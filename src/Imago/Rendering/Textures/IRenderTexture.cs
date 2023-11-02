using System;
using Veldrid;

namespace Imago.Rendering.Textures;

public interface IRenderTexture : ITexture, IDisposable
{
    /// <summary>
    /// Gets the framebuffer of the render texture.
    /// </summary>
    Framebuffer Framebuffer { get; }

    /// <summary>
    /// Gets the OutputDescription of the render texture.
    /// </summary>
    OutputDescription OutputDescription { get; }
}
