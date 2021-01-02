
using System.Drawing;
using System.IO;
using System.Numerics;
using FontStashSharp;
using FontStashSharp.Interfaces;
using LifeSim.SceneGraph;
using Veldrid;
using Rectangle = System.Drawing.Rectangle;

namespace LifeSim.Rendering
{
    public class GPURenderer2D
    {
        public class FontTextureFactory : ITexture2DCreator
        {
            private Veldrid.GraphicsDevice _gd;

            public FontTextureFactory(Veldrid.GraphicsDevice gd)
            {
                this._gd = gd;
            }

            public ITexture2D Create(int width, int height)
            {
                var factory = this._gd.ResourceFactory;
                var texture = factory.CreateTexture(new Veldrid.TextureDescription(
                    (uint) width, (uint) height, depth: 1, 
                    mipLevels: 1, arrayLayers: 1, 
                    Veldrid.PixelFormat.R8_G8_B8_A8_UNorm, 
                    Veldrid.TextureUsage.Sampled, 
                    Veldrid.TextureType.Texture2D
                ));

                var textureView = factory.CreateTextureView(texture);

                return new FontTexture2D(this._gd, texture, textureView);
            }
        }

        class FontTexture2D : GPUTexture, ITexture2D
        {
            private Veldrid.GraphicsDevice _gd;

            public FontTexture2D(Veldrid.GraphicsDevice gd, Veldrid.Texture texture, Veldrid.TextureView textureView)
                : base(texture, textureView, gd.LinearSampler)
            {
                this._gd = gd;
            }

            void ITexture2D.SetData(Rectangle bounds, byte[] data)
            {
                this._gd.UpdateTexture(
                    this._deviceTexture, data, 
                    x: (uint) bounds.X, y: (uint) bounds.Y, z: 0, 
                    width: (uint) bounds.Width, height: (uint) bounds.Height, depth: 1, 
                    mipLevel: 0, arrayLayer: 0
                );
            }
        }


        private FontTextureFactory _textureFactory;
        private GraphicsDevice _graphicsDevice;
        private SpriteBatcher _textBatcher;
        private FontSystem _fontSystem;

        private CommandList _commandList;
        private IRenderTexture _renderTexture;
        private SceneContext _sceneContext;

        public GPURenderer2D(GraphicsDevice gd, MaterialManager materialManager, SceneContext sceneContext, IRenderTexture renderTexture)
        {
            this._graphicsDevice = gd;
            this._renderTexture = renderTexture;
            this._textureFactory = new FontTextureFactory(gd);
            this._commandList = gd.ResourceFactory.CreateCommandList();
            this._sceneContext = sceneContext;
            this._textBatcher = new SpriteBatcher(gd, materialManager, this._commandList, renderTexture.outputDescription);

            this._fontSystem = this.MakeFontSystem();
			this._fontSystem.AddFont(File.ReadAllBytes(@"res/fonts/DroidSans.ttf"));
			this._fontSystem.AddFont(File.ReadAllBytes(@"res/fonts/DroidSansJapanese.ttf"));
			this._fontSystem.AddFont(File.ReadAllBytes(@"res/fonts/Symbola-Emoji.ttf"));
        }

        public FontSystem MakeFontSystem()
        {
            var fontLoader = StbTrueTypeSharpFontLoader.Instance;
            int atlasSize = 256;
			return new FontSystem(fontLoader, this._textureFactory, atlasSize, atlasSize, 0, 1, true);
        }

        public void Render(Canvas2D canvas)
        {
            Viewport viewport = canvas.viewport;
            this._commandList.Begin();
            this._commandList.SetFramebuffer(this._renderTexture.framebuffer);
            this._commandList.ClearDepthStencil(1f);
            Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0, viewport.width, viewport.height, 0, -10f, 100f);
            this._sceneContext.SetupCamera2DInfoBuffer(this._commandList, ref projection);
            this._textBatcher.BeginBatch();


            foreach (var child in canvas.children) {
                if (child is Text2D text2D) {
                    var font = this._fontSystem.GetFont(30);
                    //string text = "The quick brown fox jumps over the lazy dog\nいろはにほへ\nEmoji Font: 🙌📦👏👏";
                    font.DrawText(this._textBatcher, 30, 30, text2D.text, Color.OrangeRed);
                }
            }

            
            this._textBatcher.EndBatch();
            this._commandList.End();
        }

        public void Submit()
        {
            this._graphicsDevice.SubmitCommands(this._commandList);
        }

    }
}