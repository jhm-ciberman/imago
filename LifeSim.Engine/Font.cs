using System.IO;
using FontStashSharp;
using FontStashSharp.Interfaces;

namespace LifeSim.Engine
{
    public class Font : FontStashSharp.Interfaces.ITexture2DManager
    {
        private readonly FontSystem _fontSystem;

        public Font(string[] paths)
        {
            var fontLoader = FontStashSharp.StbTrueTypeSharpFontLoader.Instance;
            int atlasSize = 1024;
            this._fontSystem = new FontStashSharp.FontSystem(fontLoader, this, atlasSize, atlasSize, 0, 1, true);
            foreach (var path in paths)
            {
                this._fontSystem.AddFont(File.ReadAllBytes(path));
            }
        }

        public DynamicSpriteFont GetFont(int size)
        {
            return this._fontSystem.GetFont(size);
        }

        object ITexture2DManager.CreateTexture(int width, int height)
        {
            return new Rendering.Texture(width, height);
        }

        void ITexture2DManager.SetTextureData(object texture, System.Drawing.Rectangle bounds, byte[] data)
        {
            var t = (Rendering.Texture) texture;
            t.SetData(bounds.X, bounds.Y, bounds.Width, bounds.Height, data);
        }
    }
}