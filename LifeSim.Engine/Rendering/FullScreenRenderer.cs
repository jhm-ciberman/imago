using System.Numerics;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class FullScreenRenderer : System.IDisposable
    {
        private class FullScreenQuad : IRenderable, System.IDisposable
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
        private readonly GraphicsDevice _gd;


        private readonly CommandList _commandList;
        private readonly IRenderTexture _destinationTexture;
        private IRenderTexture _sourceTexture;
        private IMaterial? _material;
        private readonly PSOManager _psoManager;
        private readonly ResourceFactory _assetManager;
        private readonly Pass _pass;

        private readonly FullScreenQuad _quad;

        public FullScreenRenderer(GraphicsDevice gd, ResourceFactory assetManager, PSOManager psoManager, GPUResourceManager resources)
        {
            this._gd = gd;
            this._assetManager = assetManager;
            this._destinationTexture = resources.fullScreenRenderTexture;
            this._sourceTexture = resources.mainRenderTexture;
            this._pass = resources.fullscreenPass;
            this._sourceTexture.onResized += this._OnSourceTextureResized;
            this._psoManager = psoManager;
            this._quad = new FullScreenQuad(gd);

            var factory = gd.ResourceFactory;
            this._commandList = factory.CreateCommandList();
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
                this._material = this._assetManager.MakeSpritesMaterial(this._sourceTexture.colorTexture);
            }

            var pipeline = this._psoManager.GetPipeline(this._pass, this._material, this._quad);
            this._commandList.Begin();
            this._commandList.SetFramebuffer(this._destinationTexture.framebuffer);
            this._commandList.SetPipeline(pipeline);
            this._commandList.SetVertexBuffer(0, this._quad.vertexBuffer);
            this._commandList.SetGraphicsResourceSet(0, this._pass.resourceSet);
            this._commandList.SetGraphicsResourceSet(1, this._material.resourceSet);
            this._commandList.Draw(6);
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