using System;
using System.Collections.Generic;
using System.Numerics;
using FontStashSharp;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Controls;

public class TextBlock : Control
{
    protected string _text = string.Empty;
    protected Color _foreground = Color.Black;
    protected int _fontSize = 12;
    protected string? _fontFamily = null;
    protected int _outline = 0;
    protected int _blur = 0;

    private DynamicSpriteFont? _font = null;
    private float _lineHeight = float.NaN;
    private int _lineCount = 0;

    /// <summary>
    /// Gets or sets the text of the text block.
    /// </summary>
    public string Text
    {
        get => this._text;
        set
        {
            if (this._text != value)
            {
                this._text = value;
                this._lineCount = GetLineCount(value);
                this.InvalidateMeasure();
            }
        }
    }

    private static int GetLineCount(string value)
    {
        var lineCount = 1;
        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] == '\n')
            {
                lineCount++;
            }
        }
        return lineCount;
    }

    /// <summary>
    /// Gets or sets the foreground color of the text block.
    /// </summary>
    public Color Foreground
    {
        get => this._foreground;
        set => this.SetPropertyAndInvalidateFont(ref this._foreground, value);
    }

    /// <summary>
    /// Gets or sets the font family of the text block.
    /// </summary>
    public string? FontFamily
    {
        get => this._fontFamily;
        set => this.SetPropertyAndInvalidateFont(ref this._fontFamily, value);
    }

    /// <summary>
    /// Gets or sets the font size of the text block.
    /// </summary>
    public int FontSize
    {
        get => this._fontSize;
        set => this.SetPropertyAndInvalidateFont(ref this._fontSize, value);
    }

    /// <summary>
    /// Gets or sets the outline of the text block.
    /// </summary>
    public int Outline
    {
        get => this._outline;
        set => this.SetPropertyAndInvalidateFont(ref this._outline, value);
    }

    /// <summary>
    /// Gets or sets the blur of the text block.
    /// </summary>
    public int Blur
    {
        get => this._blur;
        set => this.SetPropertyAndInvalidateFont(ref this._blur, value);
    }

    /// <summary>
    /// Gets or sets the line height of the text block. A value of float.NaN indicates that the line height should be determined automatically.
    /// </summary>
    public float LineHeight
    {
        get => this._lineHeight;
        set
        {
            if (this._lineHeight != value)
            {
                this._lineHeight = value;
                this.InvalidateMeasure();
            }
        }
    }

    protected bool SetPropertyAndInvalidateFont<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        this._font = null;
        return true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextBlock"/> class.
    /// </summary>
    public TextBlock()
    {
        //
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextBlock"/> class.
    /// </summary>
    /// <param name="text">The text of the text block.</param>
    public TextBlock(string text)
    {
        this.Text = text;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextBlock"/> class.
    /// </summary>
    /// <param name="text">The text of the text block.</param>
    /// <param name="style">The style of the text block.</param>
    public TextBlock(string text, Style? style) : base(style)
    {
        this.Text = text;
    }

    protected SpriteFontBase GetFont()
    {
        if (this._font == null)
        {
            this._font = Font.GetFont(this.FontFamily, this.FontSize, this.Outline, this.Blur);
        }

        return this._font;
    }

    protected override Vector2 MeasureCore(Vector2 availableSize)
    {
        var font = this.GetFont();
        var size = font.MeasureString(this.Text);
        var lineHeight = float.IsNaN(this._lineHeight)
            ? font.MeasureString("M").Y
            : this._lineHeight;
        return new Vector2(size.X, lineHeight * this._lineCount);
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        base.DrawCore(spriteBatcher);

        spriteBatcher.DrawText(this.GetFont(), this.Text, this.Position, this.Foreground);
    }
}