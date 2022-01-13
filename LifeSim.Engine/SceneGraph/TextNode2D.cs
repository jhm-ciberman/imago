using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph;

public class TextNode2D : RenderNode2D
{
    public string Text;

    public Color Color = new Color(0xFFFF4500);

    public Font? Font;

    public int FontSize = 30;

    public TextNode2D() : this("") { }
    public TextNode2D(string text, Font? font = null)
    {
        this.Text = text;
        this.Font = font;
    }

    public override void Render(SpriteBatcher spriteBatcher)
    {
        if (this.Font == null) return;
        var fontSize = this.Font.GetFont(this.FontSize);
        var pos = this.WorldMatrix.Translation;
        fontSize.DrawText(spriteBatcher, this.Text, pos, this.Color);
    }
}