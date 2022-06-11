using System;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public interface IRenderTexture : ITexture, IDisposable
{
    /// <summary>
    /// Raised when the render texture is resized.
    /// </summary>
    event EventHandler? Resized;

    /// <summary>
    /// Gets the framebuffer of the render texture.
    /// </summary>
    Framebuffer Framebuffer { get; }

    /// <summary>
    /// Gets the OutputDescription of the render texture.
    /// </summary>
    OutputDescription OutputDescription { get; }

    /// <summary>
    /// Resizes the render texture.
    /// </summary>
    /// <param name="width">The new width of the render texture.</param>
    /// <param name="height">The new height of the render texture.</param>
    void Resize(uint width, uint height);
}