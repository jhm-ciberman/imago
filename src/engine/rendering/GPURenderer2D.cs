using System.Numerics;
using System.Runtime.CompilerServices;
using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class GPURenderer2D
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatcher _spriteBatcher;


        private readonly CommandList _commandList;
        private readonly IRenderTexture _renderTexture;
        private readonly SceneManager _sceneManager;
        private bool _hasCommandsToSubmit;

        public GPURenderer2D(GraphicsDevice gd, ResourceFactory assetManager, GPUResourceManager resources, PSOManager psoManager)
        {
            var renderTexture = resources.mainRenderTexture;
            this._graphicsDevice = gd;
            this._renderTexture = renderTexture;
            this._commandList = gd.ResourceFactory.CreateCommandList();
            this._sceneManager = resources.sceneManager;
            this._spriteBatcher = new SpriteBatcher(gd, psoManager, assetManager, this._commandList);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Render(Canvas2D canvas)
        {
            Viewport viewport = canvas.viewport;
            canvas.UpdateWorldMatrices();
            this._commandList.Begin();
            this._commandList.SetFramebuffer(this._renderTexture.framebuffer);
            this._commandList.ClearDepthStencil(1f);
            Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0, viewport.width, viewport.height, 0, -10f, 100f);
            this._sceneManager.SetupCamera2DInfoBuffer(this._commandList, ref projection);
            this._spriteBatcher.BeginBatch();

            this._RenderRecursive(canvas.root);

            this._spriteBatcher.EndBatch();
            this._commandList.End();

            this._hasCommandsToSubmit = true;
        }

        private void _RenderRecursive(Node2D node)
        {
            foreach (var child in node.children) {
                if (child is Renderable2D renderable) {
                    renderable.Render(this._spriteBatcher);
                    this._RenderRecursive(child);
                }
            }
        }

        public void Submit()
        {
            if (! this._hasCommandsToSubmit) return;
            this._graphicsDevice.SubmitCommands(this._commandList);
            this._hasCommandsToSubmit = false;
        }

    }
}