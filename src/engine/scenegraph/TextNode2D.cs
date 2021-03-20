using System.Drawing;
using System.Numerics;
using LifeSim.View3D;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class TextNode2D : RenderNode2D
    {
        public string text;

        public Color color = Color.OrangeRed;

        public FontAsset font;

        public int fontSize = 30;

        public TextNode2D() : this("", FontAsset.GetDefaultFont()) { }
        public TextNode2D(string text) : this(text, FontAsset.GetDefaultFont()) { }
        public TextNode2D(string text, FontAsset font)
        {
            this.text = text;
            this.font = font;
        }

        public override void Render(SpriteBatcher spriteBatcher)
        {
            //string text = "The quick brown fox jumps over the lazy dog\nいろはにほへ\nEmoji Font: 🙌📦👏👏";
            var fontSize = this.font.GetFont(this.fontSize);
            var pos = this.worldMatrix.Translation;
            fontSize.DrawText(spriteBatcher, pos.X, pos.Y, this.text, this.color);
        }
    }
}