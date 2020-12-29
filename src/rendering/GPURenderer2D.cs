
using System.Drawing;
using System.IO;
using System.Numerics;
using FontStashSharp;
using FontStashSharp.Interfaces;

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
                : base(texture, textureView)
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
        private SpriteBatcher _textBatcher;
        private FontSystem _fontSystem;

        public GPURenderer2D(Veldrid.GraphicsDevice gd, Veldrid.CommandList commandList, Veldrid.OutputDescription outputDescription)
        {
            this._textureFactory = new FontTextureFactory(gd);
            this._textBatcher = new SpriteBatcher(gd, commandList, outputDescription);

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

        public void Render(Viewport viewport)
        {
            Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0, viewport.width, viewport.height, 0, -10f, 100f);
            this._textBatcher.BeginBatch(projection);

            var font = this._fontSystem.GetFont(30);
            string text = "The quick brown fox jumps over the lazy dog\nいろはにほへ\nEmoji Font: 🙌📦👏👏";
            //string text = "Hello World!";
            font.DrawText(this._textBatcher, 30, 30, text, Color.OrangeRed);

            var atlases = this._fontSystem.Atlases;
            int i = 0;
            foreach (var atlas in atlases) {
                i++;
                this._textBatcher.Draw((GPUTexture) atlas.Texture, new Vector2(0f, 200f * i), new Vector2(200, 200));
            }

            this._textBatcher.EndBatch();
        }

    }
}