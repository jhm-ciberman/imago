using System.IO;
using FontStashSharp;
using FontStashSharp.Interfaces;

namespace LifeSim.Engine
{
    public class Font : ITexture2DCreator
    {
        private FontStashSharp.FontSystem _fontSystem;

        public Font(string[] paths)
        {
            var fontLoader = FontStashSharp.StbTrueTypeSharpFontLoader.Instance;
            int atlasSize = 1024;
            this._fontSystem = new FontStashSharp.FontSystem(fontLoader, this, atlasSize, atlasSize, 0, 1, true);
            foreach (var path in paths) {
                this._fontSystem.AddFont(File.ReadAllBytes(path));
            }
        }

        public DynamicSpriteFont GetFont(int size)
        {
            return this._fontSystem.GetFont(size);
        }

        ITexture2D ITexture2DCreator.Create(int width, int height)
        {
            return new Rendering.Texture((uint) width, (uint) height);
        }
    }
}