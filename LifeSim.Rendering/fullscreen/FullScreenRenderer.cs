using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class FullScreenRenderer : System.IDisposable
    {
        private readonly GraphicsDevice _gd;
        private readonly CommandList _commandList;

        private readonly FullscreenPass _pass;

        public FullScreenRenderer(GraphicsDevice gd, IRenderTexture sourceRenderTexture, IRenderTexture destinationRenderTexture)
        {
            this._gd = gd;

            var factory = gd.ResourceFactory;
            this._commandList = factory.CreateCommandList();
            
            this._pass = new FullscreenPass(gd, sourceRenderTexture, destinationRenderTexture);
        }

        public void Dispose()
        {
            this._commandList.Dispose();
        }

        public void Render()
        {
            this._commandList.Begin();
            this._pass.Render(this._commandList);
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