using System.IO;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Rendering
{
    public class FullScreenRenderer : System.IDisposable
    {
        private GraphicsDevice _gd;

        private DeviceBuffer _vertexBuffer;
        private CommandList _commandList;
        private IRenderTexture _destinationTexture;
        private IRenderTexture _sourceTexture;

        private Material? _material = null;
        private MaterialManager _materialManager;

        public FullScreenRenderer(GraphicsDevice gd, MaterialManager materialManager, IRenderTexture sourceRenderTexture, IRenderTexture destinationRenderTexture)
        {
            this._gd = gd;
            this._destinationTexture = destinationRenderTexture;
            this._sourceTexture = sourceRenderTexture;
            this._sourceTexture.onResized += this._OnSourceTextureResized;
            this._materialManager = materialManager;
            var factory = gd.ResourceFactory;

            this._vertexBuffer = factory.CreateBuffer(new BufferDescription(16 * 6, BufferUsage.VertexBuffer));
            (float top, float bottom) = gd.IsUvOriginTopLeft ? (1f, 0f) : (0f, 1f);
            gd.UpdateBuffer(this._vertexBuffer, 0, new[] {
                new Vector4(-1f, -1f, 0f, top    ), // x, y, u, v
                new Vector4( 1f, -1f, 1f, top    ),
                new Vector4( 1f,  1f, 1f, bottom),

                new Vector4(-1f, -1f, 0f, top   ),
                new Vector4( 1f,  1f, 1f, bottom),
                new Vector4(-1f,  1f, 0f, bottom),
            });
            this._commandList = factory.CreateCommandList();
        }

        public void Dispose()
        {
            this._vertexBuffer.Dispose();
            this._material?.Dispose();
            this._commandList.Dispose();
        }

        public void SetSourceTexture(IRenderTexture sourceRenderTexture)
        {
            this._sourceTexture.onResized -= this._OnSourceTextureResized;
            this._material?.Dispose();
            this._material = null;
            this._sourceTexture = sourceRenderTexture;
            this._sourceTexture.onResized += this._OnSourceTextureResized;
        }

        private void _OnSourceTextureResized(IRenderTexture sourceRenderTexture)
        {
            this._material?.Dispose();
            this._material = null;
        }

        public void Render()
        {
            if (this._material == null) {
                this._material = this._material = this._materialManager.MakeFullscreen(this._sourceTexture.colorTexture);
            }

            this._commandList.Begin();
            this._commandList.SetFramebuffer(this._destinationTexture.framebuffer);
            this._commandList.SetPipeline(this._material.pass.pipeline);
            this._commandList.SetVertexBuffer(0, this._vertexBuffer);
            this._commandList.SetGraphicsResourceSet(0, this._material.resourceSet);
            this._commandList.Draw(6);
            this._commandList.End();

            this._gd.SubmitCommands(this._commandList);
        }
    }
}