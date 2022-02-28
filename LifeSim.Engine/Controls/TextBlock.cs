using System;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.Resources;

namespace LifeSim.Engine.Controls;

public class TextBlock : Control
{
    public string Text { get; set; } = string.Empty;

    public Color Color { get; set; } = Color.Black;

    public Font Font { get; set; } = Font.Default;

    public int FontSize { get; set; } = 30;

    public TextBlock()
    {
        //
    }

    public TextBlock(string text)
    {
        this.Text = text;
    }

    protected override Vector2 MeasureCore(Vector2 availableSize)
    {
        var fontSystem = this.Font.GetFont(this.FontSize);
        return fontSystem.MeasureString(this.Text);
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        spriteBatcher.DrawText(this.Font, this.Text, this.FontSize, this.Position, this.Color);
    }
}