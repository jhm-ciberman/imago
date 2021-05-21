using System;
using System.Numerics;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class FullScreenRenderer : System.IDisposable
    {
        private readonly GraphicsDevice _gd;
        private readonly CommandList _commandList;
        private readonly IRenderTexture _destinationTexture;
        private IRenderTexture _sourceTexture;
        private readonly FullscreenPass _pass;
        private FullscreenMaterial _material;

        public FullScreenRenderer(GraphicsDevice gd, IRenderTexture sourceRenderTexture, IRenderTexture destinationRenderTexture)
        {
            this._gd = gd;
            this._sourceTexture = sourceRenderTexture;
            this._destinationTexture = destinationRenderTexture;

            var factory = gd.ResourceFactory;
            this._commandList = factory.CreateCommandList();
            
            this._pass = new FullscreenPass(gd, destinationRenderTexture);
            this._material = new FullscreenMaterial(ShaderRegistry.CreateFullScreenShader(gd, this._pass), this._sourceTexture);
        }

        public void Dispose()
        {
            this._commandList.Dispose();
        }

        public void SetSourceTexture(IRenderTexture sourceRenderTexture)
        {
            this._material.texture = sourceRenderTexture;
        }

        public void Render()
        {
            this._commandList.Begin();
            this._pass.Render(this._commandList, this._material);
            this._commandList.End();
        }

        public void Submit()
        {
            this._gd.SubmitCommands(this._commandList);
        }

        public void Submit(Fence fence)
        {
            this._gd.SubmitCommands(this._commandList, fence);
        }
    }
}