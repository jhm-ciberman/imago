using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.Resources;

namespace LifeSim.Engine.SceneGraph;

public class TextNode2D : RenderNode2D
{
    public string Text { get; set; } = "";

    public Color Color { get; set; } = new Color(0xFFFF4500);

    public Font Font { get; set; } = Font.Default;

    public int FontSize { get; set; } = 30;

    public TextNode2D()
    {
        //
    }

    public TextNode2D(string text, Font? font = null)
    {
        this.Text = text;
        this.Font = font ?? Font.Default;
    }

    public override void Render(SpriteBatcher spriteBatcher)
    {
        var pos = this.WorldMatrix.Translation;

        spriteBatcher.DrawText(this.Font, this.Text, this.FontSize, pos, this.Color);
    }
}