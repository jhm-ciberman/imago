using System;
using System.Numerics;
using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class CanvasRenderer : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;

        private readonly CommandList _commandList;
        private readonly IRenderTexture _renderTexture;
        private bool _hasCommandsToSubmit;
        private readonly SpritesPass _pass;

        private readonly SpriteBatcher _spriteBatcher;
        public CanvasRenderer(GraphicsDevice gd, IRenderTexture renderTexture)
        {
            this._graphicsDevice = gd;
            this._renderTexture = renderTexture;
            var factory = gd.ResourceFactory;
            this._commandList = factory.CreateCommandList();

            this._pass = new SpritesPass(gd, this._renderTexture);
            var spritesShader = ShaderRegistry.CreateSpritesShader(gd, this._pass);

            this._spriteBatcher = new SpriteBatcher(gd, spritesShader);
        }

        public void Render(Canvas2D canvas)
        {
            Viewport viewport = canvas.viewport;
            Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0, viewport.width, viewport.height, 0, -10f, 100f);

            canvas.UpdateWorldMatrices();
            this._commandList.Begin();


            this._spriteBatcher.BeginBatch();
            this._RenderRecursive(canvas.root);

            this._pass.BeginPass(this._commandList, ref projection);
            this._spriteBatcher.Submit(this._pass, this._commandList);
            this._commandList.End();

            this._hasCommandsToSubmit = true;
        }

        private void _RenderRecursive(Node2D node)
        {
            foreach (var child in node.children) {
                if (child is RenderNode2D renderable) {
                    renderable.Render(this._spriteBatcher);
                }
                this._RenderRecursive(child);
            }
        }

        public void Submit()
        {
            if (! this._hasCommandsToSubmit) return;
            this._graphicsDevice.SubmitCommands(this._commandList);
            this._hasCommandsToSubmit = false;
        }

        public void Dispose()
        {
            this._pass.Dispose();
        }
    }
}