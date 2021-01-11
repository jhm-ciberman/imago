using System.IO;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Rendering
{
    public class FullScreenRenderer : System.IDisposable
    {

        class FullScreenQuad : IRenderable, System.IDisposable
        {
            public DeviceBuffer vertexBuffer;

            public FullScreenQuad(GraphicsDevice gd)
            {
                var factory = gd.ResourceFactory;
                this.vertexBuffer = factory.CreateBuffer(new BufferDescription(16 * 6, BufferUsage.VertexBuffer));
                (float top, float bottom) = gd.IsUvOriginTopLeft ? (1f, 0f) : (0f, 1f);
                gd.UpdateBuffer(this.vertexBuffer, 0, new[] {
                    new Vector4(-1f, -1f, 0f, top    ), // x, y, u, v
                    new Vector4( 1f, -1f, 1f, top    ),
                    new Vector4( 1f,  1f, 1f, bottom),

                    new Vector4(-1f, -1f, 0f, top   ),
                    new Vector4( 1f,  1f, 1f, bottom),
                    new Vector4(-1f,  1f, 0f, bottom),
                });
            }

            public VertexLayoutKind vertexLayoutKind => VertexLayoutKind.PosOnly;

            public ResourceLayout? resourceLayout => null;

            public string[] GetShaderKeywords() => System.Array.Empty<string>();

            public void Dispose() => this.vertexBuffer.Dispose();
        }
        private GraphicsDevice _gd;


        private CommandList _commandList;
        private IRenderTexture _destinationTexture;
        private IRenderTexture _sourceTexture;
        private IMaterial? _material = null;
        private PSOManager _psoManager;
        private GPURenderer _renderer;

        private FullScreenQuad _quad;

        public FullScreenRenderer(GraphicsDevice gd, GPURenderer renderer, PSOManager psoManager, GPUResources resources)
        {
            this._gd = gd;
            this._renderer = renderer;
            this._destinationTexture = resources.fullScreenRenderTexture;
            this._sourceTexture = resources.mainRenderTexture;
            this._sourceTexture.onResized += this._OnSourceTextureResized;
            this._psoManager = psoManager;
            this._quad = new FullScreenQuad(gd);
            this._commandList = gd.ResourceFactory.CreateCommandList();
        }

        public void Dispose()
        {
            this._quad.Dispose();
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
                this._material = this._renderer.MakeFullScreenMaterial(this._sourceTexture.colorTexture);
            }

            var pipeline = this._psoManager.GetPipeline(this._material.pass, this._material, this._quad);
            this._commandList.Begin();
            this._commandList.SetFramebuffer(this._destinationTexture.framebuffer);
            this._commandList.SetPipeline(pipeline);
            this._commandList.SetVertexBuffer(0, this._quad.vertexBuffer);
            this._commandList.SetGraphicsResourceSet(0, this._material.resourceSet);
            this._commandList.Draw(6);
            this._commandList.End();
        }

        public void Submit()
        {
            this._gd.SubmitCommands(this._commandList);
        }
    }
}