using System;
using System.Numerics;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class MousePickingPass : IDisposable
{
    private readonly GraphicsDevice _gd;
    private readonly Veldrid.Texture _pixelTexture;
    public RenderTexture RenderTexture { get; set; }

    public MousePickingPass(GraphicsDevice graphicsDevice, RenderTexture renderTexture)
    {
        this._gd = graphicsDevice;
        var factory = graphicsDevice.ResourceFactory;
        this._pixelTexture = factory.CreateTexture(new TextureDescription(
            width: 1, height: 1, depth: 1, mipLevels: 1, arrayLayers: 1,
            PixelFormat.R32_UInt, TextureUsage.Staging, TextureType.Texture2D
        ));

        this.RenderTexture = renderTexture;
    }

    public uint ObjectID { get; private set; } = 0;

    private Vector2 _mousePosition;

    private bool _MouseIsInside(Vector2 mousePos)
    {
        if (mousePos.X < 0 || mousePos.Y < 0) return false;
        var texture = this.RenderTexture.PickingTexture;
        if (mousePos.X >= texture.Width || mousePos.Y >= texture.Height) return false;
        return true;
    }

    public void SetMousePosition(Vector2 mousePos)
    {
        this._mousePosition = mousePos;
    }

    public void Render(CommandList cl)
    {
        var mousePos = this._mousePosition;
        if (this._MouseIsInside(mousePos))
        {
            uint x = (uint) mousePos.X;
            uint y = this._gd.IsUvOriginTopLeft
                    ? (uint) mousePos.Y
                    : (uint) (this.RenderTexture.PickingTexture.Height - 1 - mousePos.Y);

            cl.CopyTexture(
                source: this.RenderTexture.PickingTexture,
                srcX: x, srcY: y, srcZ: 0, srcMipLevel: 0, srcBaseArrayLayer: 0,
                destination: this._pixelTexture,
                dstX: 0, dstY: 0, dstZ: 0, dstMipLevel: 0, dstBaseArrayLayer: 0,
                width: 1, height: 1, depth: 1, layerCount: 1
            );
        }

        var mappedResource = this._gd.Map<uint>(this._pixelTexture, MapMode.Read);
        this.ObjectID = mappedResource[0, 0];
        this._gd.Unmap(this._pixelTexture);
    }

    public void Dispose()
    {
        this._pixelTexture.Dispose();
    }
}