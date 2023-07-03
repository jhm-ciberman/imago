using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Imago.Rendering;
using Support;

namespace Imago.Controls;

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
    protected int _fontSize = 12;
    protected string? _fontFamily = null;
    protected Font? _font = null;
    protected Color _foreground = Color.Black;
    private float _lineHeight = float.NaN;
    private int _textLineCount = 0;
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
                this._textLineCount = GetLineCount(value);
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
    /// Gets or sets the font family of the text block.
    /// </summary>
    public string? FontFamily
    {
        get => this._fontFamily;
        set
        {
            if (this.SetPropertyAndInvalidateMeasure(ref this._fontFamily, value))
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
    public int FontSize
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
                    ? this.GetFont().LineHeight
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

    protected Font GetFont()
    {
        this._font ??= Font.GetFont(this.FontFamily, this.FontSize);

        return this._font;
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        if (this.Text == string.Empty)
        {
            return Vector2.Zero;
        }

        if (this._textLinesDirty)
        {
            this.RecomputeTextLines(availableSize);
        }

        var font = this.GetFont();
        var size = font.FontBase.MeasureString(this.Text);
        return new Vector2(size.X, this.ActualLineHeight * this._textLineCount);
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        base.DrawCore(spriteBatcher);

        var font = this.GetFont();
        var offset = new Vector2(0f, MathF.Ceiling(this._lineSpacing / 2f));
        for (var i = 0; i < this._textLines.Count; i++)
        {
            Vector2 position = this.Position + offset + new Vector2(0f, i * this.ActualLineHeight);
            if (this._textEffect == null)
            {
                spriteBatcher.DrawText(font, this._textLines[i], position, this.Foreground);
            }
            else
            {
                this._textEffect.Draw(spriteBatcher, this._textLines[i], font, position, this.Foreground);
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
            size.X = Math.Max(size.X, this.GetFont().FontBase.MeasureString(span.ToString()).X);
        }

        return size;
    }

    private static int GetLineCount(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty)
        {
            return 0;
        }

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

    // We need to split the text into lines accounting the line breaks and the width of the text block.
    private void RecomputeTextLines(Vector2 availableSize)
    {
        if (this.Text == string.Empty)
        {
            this._textLines.Clear();
            this._textLineCount = 0;
            return;
        }

        var font = this.GetFont();
        var maxWidth = availableSize.X;
        var textLines = this._textLines;

        switch (this.TextWrap)
        {
            case TextWrap.NoWrap:
                textLines.Add(this.Text);
                break;

            case TextWrap.Wrap:
                var words = this.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var currentLine = string.Empty;
                foreach (var word in words)
                {
                    var wordWidth = font.FontBase.MeasureString(word).X;
                    if (font.FontBase.MeasureString(currentLine + word).X <= maxWidth)
                    {
                        currentLine += word + " ";
                    }
                    else
                    {
                        textLines.Add(currentLine.TrimEnd());
                        currentLine = word + " ";
                    }
                }
                textLines.Add(currentLine.TrimEnd());
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        this._textLineCount = this._textLines.Count;
        this._textLinesDirty = false;
    }
}
