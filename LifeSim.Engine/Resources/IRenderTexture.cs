using System;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public interface IRenderTexture : ITexture, IDisposable
    {
        event Action<IRenderTexture>? OnResized;
        Framebuffer Framebuffer { get; }
        OutputDescription OutputDescription { get; }
        void Resize(uint width, uint height);
    }
}