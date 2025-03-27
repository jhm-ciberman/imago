using System;
using Veldrid;

namespace LifeSim.Imago.Textures;

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
