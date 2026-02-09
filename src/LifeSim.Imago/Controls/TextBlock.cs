using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using FontStashSharp;
using LifeSim.Imago.Rendering.Sprites;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Specifies how text should be wrapped within a <see cref="TextBlock"/>.
/// </summary>
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

/// <summary>
/// Specifies the horizontal alignment of text within a <see cref="TextBlock"/>.
/// </summary>
public enum TextHorizontalAlignment
{
    /// <summary>
    /// Text is aligned to the left.
    /// </summary>
    Left,

    /// <summary>
    /// Text is centered.
    /// </summary>
    Center,

    /// <summary>
    /// Text is aligned to the right.
    /// </summary>
    Right,
}

/// <summary>
/// Represents a control for displaying text.
/// </summary>
public class TextBlock : Control
{
    private string _text = string.Empty;
    private ITextEffect? _textEffect;
    private float _fontSize = 11f;
    private FontSystem? _fontSystem = null;
    private SpriteFontBase? _font = null;
    private Color _foreground = Color.Black;
    private float _lineHeight = float.NaN;
    private readonly List<string> _textLines = new();
    private bool _textLinesDirty = true;
    private TextWrap _textWrap = TextWrap.NoWrap;
    private TextHorizontalAlignment _textHorizontalAlignment = TextHorizontalAlignment.Left;

    /// <summary>
    /// Gets or sets the text content of the text block.
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
    /// Gets or sets the foreground color of the text.
    /// </summary>
    public Color Foreground
    {
        get => this._foreground;
        set => this.SetProperty(ref this._foreground, value);
    }

    /// <summary>
    /// Gets or sets the font system used to render the text.
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
    /// Gets or sets the font size of the text.
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
    /// Gets the actual height of a single line of text, including spacing.
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
    /// Gets or sets the height of each line of text. A value of <see cref="float.NaN"/> uses the default line height from the font.
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
    /// Gets or sets the text wrapping behavior.
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
    /// Gets or sets the horizontal alignment of text within the text block.
    /// </summary>
    public TextHorizontalAlignment TextHorizontalAlignment
    {
        get => this._textHorizontalAlignment;
        set => this.SetProperty(ref this._textHorizontalAlignment, value);
    }

    /// <summary>
    /// Gets or sets the text effect to apply to the text.
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
    /// Initializes a new instance of the <see cref="TextBlock"/> class with the specified text.
    /// </summary>
    /// <param name="text">The text to display.</param>
    public TextBlock(string text)
    {
        this.Text = text;
    }

    /// <summary>
    /// Gets the <see cref="SpriteFontBase"/> instance used for rendering the text,
    /// creating it from the <see cref="FontSystem"/> and <see cref="FontSize"/> if necessary.
    /// </summary>
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

    private readonly List<InlineSegment> _segments = new();
    private readonly List<int> _segmentLineStarts = new();
    private readonly List<InlineSegment> _parseBuffer = new();

    /// <inheritdoc/>
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

        var maxWidth = 0f;
        for (var i = 0; i < this._textLines.Count; i++)
        {
            var lineWidth = this.MeasureLineWidth(i);
            maxWidth = MathF.Max(maxWidth, lineWidth);
        }

        return new Vector2(maxWidth, this.ActualLineHeight * this._textLines.Count);
    }

    /// <inheritdoc/>
    protected override void DrawCore(DrawingContext ctx)
    {
        base.DrawCore(ctx);

        var font = this.Font;
        var offset = new Vector2(0f, MathF.Ceiling(this._lineSpacing / 2f));

        for (var i = 0; i < this._textLines.Count; i++)
        {
            var line = this._textLines[i];
            var lineWidth = this.MeasureLineWidth(i);

            var xOffset = this._textHorizontalAlignment switch
            {
                TextHorizontalAlignment.Left => 0f,
                TextHorizontalAlignment.Center => (this.ActualSize.X - lineWidth) / 2f,
                TextHorizontalAlignment.Right => this.ActualSize.X - lineWidth,
                _ => 0f,
            };

            Vector2 position = this.Position + offset + new Vector2(xOffset, i * this.ActualLineHeight);
            int segStart = this._segmentLineStarts[i];
            int segEnd = this._segmentLineStarts[i + 1];
            var cursorX = position.X;

            for (int s = segStart; s < segEnd; s++)
            {
                var segment = this._segments[s];
                if (segment.IsInlineObject)
                {
                    var yOffset = (this.ActualLineHeight - segment.Size.Y) / 2f;
                    ctx.DrawTexture(
                        segment.Texture!,
                        new Vector2(cursorX, position.Y + yOffset - this._lineSpacing / 2f),
                        segment.Size
                    );
                    cursorX += segment.Size.X;
                }
                else
                {
                    var text = line.Substring(segment.Start, segment.Length);
                    this.DrawTextSegment(ctx, font, text, new Vector2(cursorX, position.Y));
                    cursorX += font.MeasureString(text).X;
                }
            }
        }
    }

    private float MeasureLineWidth(int lineIndex)
    {
        var line = this._textLines[lineIndex];
        if (line.Length == 0) return 0f;

        var font = this.Font;
        int segStart = this._segmentLineStarts[lineIndex];
        int segEnd = this._segmentLineStarts[lineIndex + 1];

        var totalWidth = 0f;
        for (int s = segStart; s < segEnd; s++)
        {
            var seg = this._segments[s];
            totalWidth += seg.IsInlineObject
                ? seg.Size.X
                : font.MeasureString(line.Substring(seg.Start, seg.Length)).X;
        }

        return totalWidth;
    }

    private void DrawTextSegment(DrawingContext ctx, SpriteFontBase font, string text, Vector2 position)
    {
        if (this._textEffect == null)
        {
            ctx.DrawText(font, text, position, this.Foreground);
        }
        else
        {
            this._textEffect.Draw(ctx, text, font, position, this.Foreground);
        }
    }

    /// <summary>
    /// Measures the width of the first <paramref name="charNumber"/> characters of the text,
    /// accounting for inline objects and surrogate pairs.
    /// </summary>
    /// <param name="charNumber">The number of characters to measure.</param>
    /// <returns>The size of the measured substring.</returns>
    internal Vector2 MeasureString(int charNumber)
    {
        if (this.Text == string.Empty || charNumber <= 0)
        {
            return Vector2.Zero;
        }

        if (this._textLinesDirty)
        {
            this.RecomputeTextLines(this.ActualSize);
        }

        var font = this.Font;
        var size = Vector2.Zero;

        for (var i = 0; i < this._textLines.Count; i++)
        {
            var line = this._textLines[i];
            var measureTo = Math.Min(charNumber, line.Length);

            // Never split a surrogate pair.
            if (measureTo > 0 && measureTo < line.Length && char.IsLowSurrogate(line[measureTo]))
            {
                measureTo--;
            }

            int segStart = this._segmentLineStarts[i];
            int segEnd = this._segmentLineStarts[i + 1];
            float width = 0f;
            int charsCounted = 0;

            for (int s = segStart; s < segEnd; s++)
            {
                if (charsCounted >= measureTo) break;

                var seg = this._segments[s];
                if (seg.IsInlineObject)
                {
                    if (charsCounted + seg.Length <= measureTo)
                    {
                        width += seg.Size.X;
                    }

                    charsCounted += seg.Length;
                }
                else
                {
                    int segChars = Math.Min(seg.Length, measureTo - charsCounted);
                    width += font.MeasureString(line.Substring(seg.Start, segChars)).X;
                    charsCounted += segChars;
                }
            }

            size.X = Math.Max(size.X, width);
        }

        return size;
    }

    /// <summary>
    /// Recomputes the internal list of text lines based on the current <see cref="Text"/>,
    /// <see cref="TextWrap"/> mode, and available width.
    /// </summary>
    /// <param name="availableSize">The available size for text layout.</param>
    private void RecomputeTextLines(Vector2 availableSize)
    {
        this._textLines.Clear();
        if (this.Text == string.Empty)
        {
            this._segments.Clear();
            this._segmentLineStarts.Clear();
            this._segmentLineStarts.Add(0);
            this._textLinesDirty = false;
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

        this.RecomputeSegments();
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

    // FontBase.MeasureString do not support ReadOnlySpan<char>, so we use this ugly StringBuilder to allocate a little less memory.
    private static readonly StringBuilder _measureStringBuilder = new();

    /// <summary>
    /// Applies word wrapping to the text, breaking lines at word boundaries to fit within the specified maximum width.
    /// </summary>
    /// <param name="maxWidth">The maximum width available for a line of text.</param>
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

    private void RecomputeSegments()
    {
        this._segments.Clear();
        this._segmentLineStarts.Clear();

        var provider = Visual.DefaultInlineObjectProvider;

        for (int i = 0; i < this._textLines.Count; i++)
        {
            this._segmentLineStarts.Add(this._segments.Count);
            var line = this._textLines[i];

            if (line.Length == 0)
            {
                continue;
            }

            if (provider != null)
            {
                provider.Parse(line, this.FontSize, this._parseBuffer);
                this._segments.AddRange(this._parseBuffer);
            }
            else
            {
                this._segments.Add(new InlineSegment(0, line.Length));
            }
        }

        this._segmentLineStarts.Add(this._segments.Count);
    }

    /// <summary>
    /// Finds the inline object segment that contains the given character index, if any.
    /// </summary>
    /// <param name="charIndex">The character index to test.</param>
    /// <param name="segment">When this method returns <c>true</c>, the matching inline segment.</param>
    /// <returns><c>true</c> if the character is inside an inline object segment; otherwise <c>false</c>.</returns>
    internal bool TryGetInlineSegmentAt(int charIndex, out InlineSegment segment)
    {
        segment = default;

        if (this._textLinesDirty)
        {
            this.RecomputeTextLines(this.ActualSize);
        }

        for (int i = 0; i < this._segments.Count; i++)
        {
            var seg = this._segments[i];
            if (seg.IsInlineObject && charIndex >= seg.Start && charIndex < seg.Start + seg.Length)
            {
                segment = seg;
                return true;
            }
        }

        return false;
    }
}
