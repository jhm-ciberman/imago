using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class MousePickingPass : IDisposable, IRenderingPass
{
    private readonly GraphicsDevice _gd;
    private readonly Veldrid.Texture _pixelTexture;
    internal RenderTexture RenderTexture { get; set; }

    private uint _pickingIdCounter = 0; // 0 is reserved for "no object"

    private readonly Dictionary<uint, RenderNode3D> _pickingIdToRenderNode = new Dictionary<uint, RenderNode3D>();


    public MousePickingPass(Renderer renderer, RenderTexture renderTexture)
    {
        this._gd = renderer.GraphicsDevice;
        var factory = this._gd.ResourceFactory;

        // This is a 1x1 texture that will be used to read the pixel color from the mouse picking pass.
        this._pixelTexture = factory.CreateTexture(new TextureDescription(
            width: 1, height: 1, depth: 1, mipLevels: 1, arrayLayers: 1,
            PixelFormat.R32_UInt, TextureUsage.Staging, TextureType.Texture2D
        ));

        this.RenderTexture = renderTexture;
    }

    public uint ObjectID { get; private set; } = 0;

    private Vector2 _mousePosition;

    private bool MouseIsInside(Vector2 mousePos)
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

    public RenderNode3D? SelectedRenderNode
    {
        get
        {
            if (this.ObjectID == 0) return null;
            this._pickingIdToRenderNode.TryGetValue(this.ObjectID, out var renderNode);
            return renderNode;
        }
    }

    public void Render(CommandList cl, Scene scene)
    {
        var mousePos = this._mousePosition;
        if (this.MouseIsInside(mousePos))
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

    public uint RegisterPickable(RenderNode3D renderNode)
    {
        if (this._pickingIdCounter == uint.MaxValue)
        {
            throw new InvalidOperationException("Picking ID counter overflow.");
        }

        var pickingId = ++this._pickingIdCounter;
        this._pickingIdToRenderNode.Add(pickingId, renderNode);
        return pickingId;
    }

    public void UnregisterPickable(uint pickingId)
    {
        this._pickingIdToRenderNode.Remove(pickingId);
    }
}