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

    protected override void MeasureCore(Vector2 availableSize)
    {
        var fontSystem = this.Font.GetFont(this.FontSize);
        var size = fontSystem.MeasureString(this.Text);
        size.X = MathF.Min(availableSize.X, size.X);
        size.Y = MathF.Min(availableSize.Y, size.Y);
        this.DesiredSize = size;
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        spriteBatcher.DrawText(this.Font, this.Text, this.FontSize, this.Position, this.Color);
    }
}