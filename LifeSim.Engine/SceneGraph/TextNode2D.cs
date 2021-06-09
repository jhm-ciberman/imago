using System.Drawing;
using FontStashSharp;
using LifeSim.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class TextNode2D : RenderNode2D
    {
        public string text;

        public Color color = new Color(0xFFFF4500);

        public Font? font;

        public int fontSize = 30;

        public TextNode2D() : this("") { }
        public TextNode2D(string text, Font? font = null)
        {
            this.text = text;
            this.font = font;
        }

        public override void Render(SpriteBatcher spriteBatcher)
        {
            if (this.font == null) return;
            var fontSize = this.font.GetFont(this.fontSize);
            var pos = this.worldMatrix.Translation;
            fontSize.DrawText(spriteBatcher, pos.X, pos.Y, this.text, this.color);
        }
    }
}