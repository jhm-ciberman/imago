using System;
using System.Numerics;
using FontStashSharp;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.Resources;

namespace LifeSim.Engine.Controls;

public class TextBlock : Control
{
    public string Text { get; set; } = string.Empty;

    public Color Color { get; set; } = Color.Black;

    public string? FontFamily { get; set; }

    public int FontSize { get; set; } = 30;

    public int Outline { get; set; } = 0;

    public int Blur { get; set; } = 0;

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
        return Font.GetFont(this.FontFamily, this.FontSize).MeasureString(this.Text);
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        var font = Font.GetFont(this.FontFamily, this.FontSize, this.Outline, this.Blur);
        spriteBatcher.DrawText(font, this.Text, this.Position, this.Color);
    }
}