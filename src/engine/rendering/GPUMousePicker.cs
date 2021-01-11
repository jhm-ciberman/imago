using System.Numerics;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class GPUMousePicker
    {
        private GraphicsDevice _gd;
        private Texture _pixelTexture;
        private CommandList _commandList;

        private uint _objectID = 0;
        private bool _hasCommandsToSubmit = false;

        public GPUMousePicker(GraphicsDevice graphicsDevice)
        {
            this._gd = graphicsDevice;
            var factory = graphicsDevice.ResourceFactory;
            this._pixelTexture = factory.CreateTexture(new TextureDescription(
                width: 1, height: 1, depth: 1, mipLevels: 1, arrayLayers: 1, 
                PixelFormat.R32_UInt, TextureUsage.Staging, TextureType.Texture2D
            ));

            this._commandList = factory.CreateCommandList();
        }

        public uint objectID => this._objectID;

        private bool _MouseIsInside(RenderTexture mainRenderTexture, Vector2 mousePos)
        {
            if (mousePos.X < 0) return false;
            if (mousePos.Y < 0) return false;
            var texture = mainRenderTexture.pickingTexture;
            if (mousePos.X >= texture.Width) return false;
            if (mousePos.Y >= texture.Height) return false;
            return true;
        }

        public void Update(RenderTexture mainRenderTexture, Vector2 mousePos)
        {
            if (this._MouseIsInside(mainRenderTexture, mousePos)) {
                uint x = (uint) mousePos.X;
                uint y;
                if (this._gd.IsUvOriginTopLeft) {
                    y = (uint) (mousePos.Y);
                } else {
                    y = (uint) (mainRenderTexture.pickingTexture.Height - 1 - mousePos.Y);
                }
                this._commandList.Begin();
                this._commandList.CopyTexture(
                    source: mainRenderTexture.pickingTexture, 
                    srcX: x, srcY: y, srcZ: 0, srcMipLevel: 0, srcBaseArrayLayer: 0, 
                    destination: this._pixelTexture, 
                    dstX: 0, dstY: 0, dstZ: 0, dstMipLevel: 0, dstBaseArrayLayer: 0, 
                    width: 1, height: 1, depth: 1, layerCount: 1
                );
                this._commandList.End();
                this._hasCommandsToSubmit = true;
            }

            var mappedResource = this._gd.Map<uint>(this._pixelTexture, MapMode.Read);
            this._objectID = mappedResource[0, 0];
            this._gd.Unmap(this._pixelTexture);
        }

        public void Submit()
        {
            if (! this._hasCommandsToSubmit) return;
            
            this._gd.SubmitCommands(this._commandList);
            this._hasCommandsToSubmit = false;
        }
    }
}