using LifeSim.Engine.Rendering;
using LifeSim.Utils;

namespace LifeSim.Engine.SceneGraph;

public class TextNode2D : RenderNode2D
{
    public string Text { get; set; } = "";

    public Color Color { get; set; } = new Color(0xFFFF4500);

    public string? FontFamily { get; set; }

    public int FontSize { get; set; } = 30;
    public TextNode2D()
    {
        //
    }

    public TextNode2D(string text, string? fontFamily = null)
    {
        this.Text = text;
        this.FontFamily = fontFamily;
    }

    public override void Render(SpriteBatcher spriteBatcher)
    {
        var pos = this.WorldMatrix.Translation;

        var font = Font.GetFont(this.FontFamily, this.FontSize);
        spriteBatcher.DrawText(font, this.Text, pos, this.Color);
    }
}