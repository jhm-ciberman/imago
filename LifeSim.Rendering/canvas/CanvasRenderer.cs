using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace LifeSim.Rendering
{
    public class CanvasRenderer : IDisposable
    {
        private readonly GraphicsDevice _gd;

        private readonly CommandList _commandList;
        private readonly IRenderTexture _renderTexture;
        private bool _hasCommandsToSubmit;
        private readonly SpritesPass _pass;

        private readonly SpriteBatcher _spriteBatcher;
        public CanvasRenderer(GraphicsDevice gd, IRenderTexture renderTexture)
        {
            this._gd = gd;
            this._renderTexture = renderTexture;
            var factory = gd.ResourceFactory;
            this._commandList = factory.CreateCommandList();

            this._pass = new SpritesPass(gd, this._renderTexture);

            this._spriteBatcher = new SpriteBatcher(gd, this._pass.Shader);
        }

        public void Render(Viewport viewport, IReadOnlyList<ICanvasItem> items)
        {
            Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, -10f, 100f);

            this._spriteBatcher.BeginBatch();
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Render(this._spriteBatcher);
            }

            this._commandList.Begin();
            this._pass.BeginPass(this._commandList, ref projection);
            this._pass.SubmitBatches(this._commandList, this._spriteBatcher.IndexBuffer, this._spriteBatcher.Batches);
            this._commandList.End();


            this._hasCommandsToSubmit = true;
        }

        public void Submit()
        {
            if (!this._hasCommandsToSubmit) return;
            this._gd.SubmitCommands(this._commandList);
            this._hasCommandsToSubmit = false;
        }

        public void Dispose()
        {
            this._pass.Dispose();
        }
    }
}