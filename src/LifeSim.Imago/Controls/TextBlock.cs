using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using FontStashSharp;
using LifeSim.Imago.Rendering.Sprites;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.Controls;

public enum TextWrap
{
    /// <summary>
    /// The text is not wrapped.
    /// </summary>
    NoWrap,

    /// <summary>
    /// The text is wrapped at the nearest word boundary.
    /// </summary>
    Wrap,
}

public class TextBlock : Control
{
    protected string _text = string.Empty;
    private ITextEffect? _textEffect;
    protected float _fontSize = 11f;
    protected FontSystem? _fontSystem = null;
    protected SpriteFontBase? _font = null;
    protected Color _foreground = Color.Black;
    private float _lineHeight = float.NaN;
    private readonly List<string> _textLines = new();
    private bool _textLinesDirty = true;
    private TextWrap _textWrap = TextWrap.NoWrap;

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
                this._textLinesDirty = true;
                this.InvalidateMeasure();
                this.OnPropertyChanged(nameof(this.Text));
            }
        }
    }

    /// <summary>
    /// Gets or sets the foreground color of the text block.
    /// </summary>
    public Color Foreground
    {
        get => this._foreground;
        set => this.SetProperty(ref this._foreground, value);
    }

    /// <summary>
    /// Gets or sets the font system of the text block.
    /// </summary>
    public FontSystem? FontSystem
    {
        get => this._fontSystem;
        set
        {
            if (this.SetPropertyAndInvalidateMeasure(ref this._fontSystem, value))
            {
                this._actualLineHeight = float.NaN;
                this._font = null;
                this._textLinesDirty = true;
                this.InvalidateMeasure();
            }
        }
    }

    /// <summary>
    /// Gets or sets the font size of the text block.
    /// </summary>
    public float FontSize
    {
        get => this._fontSize;
        set
        {
            if (this.SetPropertyAndInvalidateMeasure(ref this._fontSize, value))
            {
                this._actualLineHeight = float.NaN;
                this._font = null;
                this._textLinesDirty = true;
                this.InvalidateMeasure();
            }
        }
    }

    private float _actualLineHeight = float.NaN;

    private float _lineSpacing = 0f;

    /// <summary>
    /// Gets the actual height of each line of text.
    /// </summary>
    public float ActualLineHeight
    {
        get
        {
            if (float.IsNaN(this._actualLineHeight))
            {
                this._actualLineHeight = float.IsNaN(this._lineHeight)
                    ? this.Font.LineHeight
                    : this._lineHeight;
                this._lineSpacing = this._actualLineHeight - this.FontSize;
            }

            return this._actualLineHeight;
        }
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
                this._actualLineHeight = float.NaN;
                this._textLinesDirty = true;
                this.InvalidateMeasure();
            }
        }
    }

    /// <summary>
    /// Gets or sets the text wrap of the text block.
    /// </summary>
    public TextWrap TextWrap
    {
        get => this._textWrap;
        set
        {
            if (this._textWrap != value)
            {
                this._textWrap = value;
                this._textLinesDirty = true;
                this.InvalidateMeasure();
            }
        }
    }

    /// <summary>
    /// Gets or sets the text effect of the text block.
    /// </summary>
    public ITextEffect? TextEffect
    {
        get => this._textEffect;
        set
        {
            if (this.SetPropertyAndInvalidateMeasure(ref this._textEffect, value))
            {
                this._font = null;
                this._actualLineHeight = float.NaN;
            }
        }
    }

    private void TextEffect_FontChanged(object? sender, EventArgs e)
    {
        // The font of the text effect has changed, so we need to invalidate the measure and the cached font.
        this._font = null;
        this._actualLineHeight = float.NaN;
        this.InvalidateMeasure();
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

    protected SpriteFontBase Font
    {
        get
        {
            if (this._font != null)
            {
                return this._font;
            }

            var system = this.FontSystem
                ?? Visual.DefaultFontSystem
                ?? throw new InvalidOperationException("FontSystem is not set.");

            this._font = system.GetFont(this.FontSize);

            return this._font;
        }
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        if (this._textLinesDirty)
        {
            this.RecomputeTextLines(availableSize);
        }

        if (this.Text == string.Empty)
        {
            return Vector2.Zero;
        }

        var size = this.Font.MeasureString(this.Text);
        return new Vector2(size.X, this.ActualLineHeight * this._textLines.Count);
    }

    protected override void DrawCore(DrawingContext ctx)
    {
        base.DrawCore(ctx);

        var font = this.Font;
        var offset = new Vector2(0f, MathF.Ceiling(this._lineSpacing / 2f));
        for (var i = 0; i < this._textLines.Count; i++)
        {
            Vector2 position = this.Position + offset + new Vector2(0f, i * this.ActualLineHeight);
            if (this._textEffect == null)
            {
                ctx.DrawText(font, this._textLines[i], position, this.Foreground);
            }
            else
            {
                this._textEffect.Draw(ctx, this._textLines[i], font, position, this.Foreground);
            }
        }
    }

    internal Vector2 MeasureString(int charNumber)
    {
        if (this.Text == string.Empty)
        {
            return Vector2.Zero;
        }

        if (this._textLinesDirty)
        {
            this.RecomputeTextLines(this.ActualSize);
        }

        var size = Vector2.Zero;
        for (var i = 0; i < this._textLines.Count; i++)
        {
            var span = this._textLines[i].AsSpan(0, charNumber);
            size.X = Math.Max(size.X, this.Font.MeasureString(span.ToString()).X);
        }

        return size;
    }

    // We need to split the text into lines accounting the line breaks and the width of the text block.
    private void RecomputeTextLines(Vector2 availableSize)
    {
        this._textLines.Clear();
        if (this.Text == string.Empty)
        {
            return;
        }

        switch (this.TextWrap)
        {
            case TextWrap.NoWrap:
                this.ApplyNoWrap();
                break;

            case TextWrap.Wrap:
                this.ApplyWrap(availableSize.X);
                break;

            default:
                throw new NotSupportedException();
        }

        this._textLinesDirty = false;
    }

    private void ApplyNoWrap()
    {
        var lineStart = 0;
        for (var i = 0; i < this.Text.Length; i++)
        {
            if (this.Text[i] == '\n')
            {
                this._textLines.Add(this.Text.AsSpan(lineStart, i - lineStart).ToString());
                lineStart = i + 1;
            }
        }

        if (lineStart < this.Text.Length)
        {
            this._textLines.Add(this.Text.AsSpan(lineStart).ToString());
        }
    }

    // FontBase.MeasureString do not LifeSim.Support ReadOnlySpan<char>, so we use this ugly StringBuilder to allocate a little less memory.
    private static readonly StringBuilder _measureStringBuilder = new();

    private void ApplyWrap(float maxWidth)
    {
        var sb = _measureStringBuilder;
        var font = this.Font;
        var lineStart = 0;
        var lineLength = 0;
        var lastSpace = -1;
        for (var i = 0; i < this.Text.Length; i++)
        {
            char c = this.Text[i];
            if (c == '\n')
            {
                this._textLines.Add(this.Text.AsSpan(lineStart, lineLength).ToString());
                lineStart = i + 1;
                lineLength = 0;
                lastSpace = -1;
            }
            else if (c == ' ')
            {
                lastSpace = i;
            }

            lineLength++;
            sb.Clear();
            sb.Append(this.Text.AsSpan(lineStart, Math.Min(lineLength, this.Text.Length - lineStart)));
            if (font.MeasureString(sb).X > maxWidth)
            {
                if (lastSpace != -1)
                {
                    this._textLines.Add(this.Text.AsSpan(lineStart, lastSpace - lineStart).ToString());
                    lineStart = lastSpace + 1;
                    lineLength = i - lastSpace;
                    lastSpace = -1;
                }
                else
                {
                    this._textLines.Add(this.Text.AsSpan(lineStart, lineLength - 1).ToString());
                    lineStart = i;
                    lineLength = 1;
                }
            }
        }

        if (lineStart < this.Text.Length)
        {
            this._textLines.Add(this.Text.AsSpan(lineStart).ToString());
        }
    }
}
