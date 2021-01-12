using System.IO;
using FontStashSharp;
using LifeSim.Engine;

namespace LifeSim.Assets
{
    public class FontAsset : IAsset
    {
        private static FontAsset? _default = null;

        public static FontAsset GetDefaultFont()
        {
            if (FontAsset._default == null) {
                throw new System.Exception("No default font loaded");
            }
            return FontAsset._default;
        }

        private FontSystem _fontSystem;

        public FontAsset(FontSystem fontSystem)
        {
            if (FontAsset._default == null) {
                FontAsset._default = this;
            }
            this._fontSystem = fontSystem;
        }

        public DynamicSpriteFont GetFont(int fontSize)
        {
            return this._fontSystem.GetFont(fontSize);
        }
    }
}