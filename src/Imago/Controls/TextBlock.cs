using System;
using System.Numerics;
using FontStashSharp;
using Imago.Rendering.Sprites;
using Imago.Support.Drawing;

namespace Imago.Controls;

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
[TextProperty(nameof(Text))]
public class TextBlock : Control
{
    private readonly TextLayout _layout = new();
    private ITextEffect? _textEffect;
    private TextFont? _textFont;
    private FontSystem? _fontSystem = null;
    private SpriteFontBase? _font = null;
    private Color _foreground = Color.Black;
    private TextHorizontalAlignment _textHorizontalAlignment = TextHorizontalAlignment.Left;

    /// <summary>
    /// Gets or sets the text content of the text block.
    /// </summary>
    public string Text
    {
        get => this._layout.Text;
        set
        {
            if (this._layout.Text != value)
            {
                this._layout.Text = value;
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
    /// Gets or sets the font used to render the text. Setting this property
    /// applies the font's <see cref="TextFont.System"/>, <see cref="TextFont.Size"/>,
    /// and optionally <see cref="TextFont.LineHeight"/> to this control.
    /// Setting <see cref="FontSystem"/> or <see cref="FontSize"/> individually
    /// clears this property.
    /// </summary>
    public TextFont? Font
    {
        get => this._textFont;
        set
        {
            if (this._textFont == value)
            {
                return;
            }

            this._textFont = value;

            if (value != null)
            {
                this._fontSystem = value.System;
                this._layout.FontSize = value.Size;

                if (!float.IsNaN(value.LineHeight))
                {
                    this._layout.LineHeight = value.LineHeight;
                }
            }

            this._font = null;
            this.InvalidateMeasure();
            this.OnPropertyChanged(nameof(this.Font));
        }
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
                this._textFont = null;
                this._font = null;
            }
        }
    }

    /// <summary>
    /// Gets or sets the font size of the text.
    /// </summary>
    public float FontSize
    {
        get => this._layout.FontSize;
        set
        {
            if (this._layout.FontSize != value)
            {
                this._layout.FontSize = value;
                this._textFont = null;
                this._font = null;
                this.InvalidateMeasure();
                this.OnPropertyChanged(nameof(this.FontSize));
            }
        }
    }

    /// <summary>
    /// Gets the actual height of a single line of text, including spacing.
    /// </summary>
    public float ActualLineHeight
    {
        get
        {
            this.EnsureLayoutFont();
            return this._layout.ActualLineHeight;
        }
    }

    /// <summary>
    /// Gets or sets the height of each line of text. A value of <see cref="float.NaN"/> uses the default line height from the font.
    /// </summary>
    public float LineHeight
    {
        get => this._layout.LineHeight;
        set
        {
            if (this._layout.LineHeight != value)
            {
                this._layout.LineHeight = value;
                this.InvalidateMeasure();
            }
        }
    }

    /// <summary>
    /// Gets or sets the text wrapping behavior.
    /// </summary>
    public TextWrap TextWrap
    {
        get => this._layout.TextWrap;
        set
        {
            if (this._layout.TextWrap != value)
            {
                this._layout.TextWrap = value;
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
            }
        }
    }

    private void TextEffect_FontChanged(object? sender, EventArgs e)
    {
        this._font = null;
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
    /// <remarks>
    /// When neither <see cref="Font"/> nor <see cref="FontSystem"/> has been set on this
    /// control and <see cref="Visual.DefaultFont"/> is non-null, the default font's system,
    /// size, and line height are applied here. This lets a project provide a sensible
    /// fallback for unconfigured text without imposing a specific style at the engine level.
    /// </remarks>
    protected SpriteFontBase ResolvedFont
    {
        get
        {
            if (this._font != null)
            {
                return this._font;
            }

            if (this._textFont == null && this._fontSystem == null && Visual.DefaultFont is { } defaultFont)
            {
                this._fontSystem = defaultFont.System;
                this._layout.FontSize = defaultFont.Size;
                if (!float.IsNaN(defaultFont.LineHeight))
                {
                    this._layout.LineHeight = defaultFont.LineHeight;
                }
            }

            var system = this._fontSystem ?? Visual.GetDefaultFontSystemOrFail();

            this._font = system.GetFont(this.FontSize);

            return this._font;
        }
    }

    /// <inheritdoc/>
    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        this.EnsureLayoutFont();
        this._layout.AvailableWidth = availableSize.X;

        if (this._layout.Text.Length == 0)
        {
            return Vector2.Zero;
        }

        var maxWidth = 0f;
        for (var i = 0; i < this._layout.LineCount; i++)
        {
            maxWidth = MathF.Max(maxWidth, this._layout.GetLineWidth(i));
        }

        return new Vector2(maxWidth, this._layout.ActualLineHeight * this._layout.LineCount);
    }

    /// <inheritdoc/>
    protected override void DrawCore(DrawingContext ctx)
    {
        base.DrawCore(ctx);

        var font = this.ResolvedFont;
        var layout = this._layout;
        var offset = new Vector2(0f, MathF.Ceiling(layout.LineSpacing / 2f));

        for (var i = 0; i < layout.LineCount; i++)
        {
            var line = layout.GetLineText(i);
            var lineWidth = layout.GetLineWidth(i);

            var xOffset = this._textHorizontalAlignment switch
            {
                TextHorizontalAlignment.Left => 0f,
                TextHorizontalAlignment.Center => (this.ActualSize.X - lineWidth) / 2f,
                TextHorizontalAlignment.Right => this.ActualSize.X - lineWidth,
                _ => 0f,
            };

            Vector2 position = this.Position + offset + new Vector2(xOffset, i * layout.ActualLineHeight);
            int segCount = layout.GetLineSegmentCount(i);
            var cursorX = position.X;

            for (int s = 0; s < segCount; s++)
            {
                var segment = layout.GetLineSegment(i, s);
                if (segment.IsInlineContent)
                {
                    var yOffset = (layout.ActualLineHeight - segment.Size.Y) / 2f;
                    ctx.DrawTexture(
                        segment.Texture!,
                        new Vector2(cursorX, position.Y + yOffset - layout.LineSpacing / 2f),
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
    /// accounting for inline contents and surrogate pairs.
    /// </summary>
    /// <param name="charNumber">The number of characters to measure.</param>
    /// <returns>The size of the measured substring.</returns>
    internal Vector2 MeasureString(int charNumber)
    {
        this.EnsureLayoutFont();
        return new Vector2(this._layout.MeasureChars(charNumber), 0f);
    }

    /// <summary>
    /// Finds the inline content segment that contains the given character index, if any.
    /// </summary>
    /// <param name="charIndex">The character index to test.</param>
    /// <param name="segment">When this method returns <c>true</c>, the matching inline segment.</param>
    /// <returns><c>true</c> if the character is inside an inline content segment; otherwise <c>false</c>.</returns>
    internal bool TryGetInlineSegmentAt(int charIndex, out InlineSegment segment)
    {
        this.EnsureLayoutFont();
        return this._layout.TryGetInlineSegmentAt(charIndex, out segment);
    }

    private void EnsureLayoutFont()
    {
        this._layout.Font = this.ResolvedFont;
    }
}
