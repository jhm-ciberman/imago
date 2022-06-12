using System;
using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Controls;

public class TextBlock : Control
{
    protected string _text = string.Empty;

    /// <summary>
    /// Gets or sets the text of the text block.
    /// </summary>
    public string Text
    {
        get => this._text;
        set => this._text = value;
    }

    public Color Foreground { get; set; } = Color.Black;

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
        return Font.GetFont(this.FontFamily, this.FontSize, this.Outline, this.Blur).MeasureString(this.Text);
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        base.DrawCore(spriteBatcher);

        var font = Font.GetFont(this.FontFamily, this.FontSize, this.Outline, this.Blur);
        spriteBatcher.DrawText(font, this.Text, this.Position, this.Foreground);
    }
}