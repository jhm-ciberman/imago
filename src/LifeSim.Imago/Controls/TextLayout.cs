using System;
using System.Collections.Generic;
using System.Text;
using FontStashSharp;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Specifies how text should be wrapped within a <see cref="TextLayout"/>.
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
/// A self-contained text layout engine that splits text into lines and segments,
/// caches measurements, and supports inline objects. Setting any input property
/// marks the layout dirty; reading any output triggers lazy recomputation.
/// </summary>
public class TextLayout
{
    private string _text = string.Empty;
    private SpriteFontBase? _font;
    private float _fontSize = 11f;
    private float _lineHeight = float.NaN;
    private TextWrap _textWrap = TextWrap.NoWrap;
    private IInlineObjectProvider? _inlineObjectProvider;
    private float _availableWidth;

    /// <summary>
    /// Gets or sets the default inline object provider used by all <see cref="TextLayout"/> instances.
    /// When set, layouts without an explicit <see cref="InlineObjectProvider"/> will fall back to this.
    /// </summary>
    public static IInlineObjectProvider? DefaultInlineObjectProvider { get; set; }

    private bool _isDirty = true;
    private float _actualLineHeight;
    private float _lineSpacing;

    private bool SetProperty<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        this._isDirty = true;
        return true;
    }

    private readonly List<string> _lines = new();
    private readonly List<InlineSegment> _segments = new();
    private readonly List<int> _segmentLineStarts = new();
    private readonly List<InlineSegment> _parseBuffer = new();

    // FontStashSharp doesn't support ReadOnlySpan<char> for MeasureString.
    private static readonly StringBuilder _wrapMeasureBuilder = new();

    /// <summary>
    /// Gets or sets the text content.
    /// </summary>
    public string Text
    {
        get => this._text;
        set => this.SetProperty(ref this._text, value);
    }

    /// <summary>
    /// Gets or sets the resolved font used for measurement.
    /// </summary>
    public SpriteFontBase? Font
    {
        get => this._font;
        set => this.SetProperty(ref this._font, value);
    }

    /// <summary>
    /// Gets or sets the font size, passed to the inline object provider for sizing.
    /// </summary>
    public float FontSize
    {
        get => this._fontSize;
        set => this.SetProperty(ref this._fontSize, value);
    }

    /// <summary>
    /// Gets or sets the height of each line of text.
    /// A value of <see cref="float.NaN"/> uses the default line height from the font.
    /// </summary>
    public float LineHeight
    {
        get => this._lineHeight;
        set => this.SetProperty(ref this._lineHeight, value);
    }

    /// <summary>
    /// Gets or sets the text wrapping behavior.
    /// </summary>
    public TextWrap TextWrap
    {
        get => this._textWrap;
        set => this.SetProperty(ref this._textWrap, value);
    }

    /// <summary>
    /// Gets or sets the inline object provider used to detect and size inline objects within text.
    /// </summary>
    public IInlineObjectProvider? InlineObjectProvider
    {
        get => this._inlineObjectProvider;
        set => this.SetProperty(ref this._inlineObjectProvider, value);
    }

    /// <summary>
    /// Gets or sets the available width for word wrapping.
    /// </summary>
    public float AvailableWidth
    {
        get => this._availableWidth;
        set => this.SetProperty(ref this._availableWidth, value);
    }

    /// <summary>
    /// Gets the number of computed lines.
    /// </summary>
    public int LineCount
    {
        get
        {
            this.EnsureUpToDate();
            return this._lines.Count;
        }
    }

    /// <summary>
    /// Gets the actual height of a single line of text, including spacing.
    /// </summary>
    public float ActualLineHeight
    {
        get
        {
            this.EnsureUpToDate();
            return this._actualLineHeight;
        }
    }

    /// <summary>
    /// Gets the spacing between the font size and the actual line height.
    /// </summary>
    public float LineSpacing
    {
        get
        {
            this.EnsureUpToDate();
            return this._lineSpacing;
        }
    }

    /// <summary>
    /// Gets the text of the specified line.
    /// </summary>
    /// <param name="lineIndex">The zero-based line index.</param>
    /// <returns>The text content of the line.</returns>
    public string GetLineText(int lineIndex)
    {
        this.EnsureUpToDate();
        return this._lines[lineIndex];
    }

    /// <summary>
    /// Gets the pixel width of the specified line.
    /// </summary>
    /// <param name="lineIndex">The zero-based line index.</param>
    /// <returns>The width of the line in pixels.</returns>
    public float GetLineWidth(int lineIndex)
    {
        this.EnsureUpToDate();
        var line = this._lines[lineIndex];
        if (line.Length == 0) return 0f;

        int segStart = this._segmentLineStarts[lineIndex];
        int segEnd = this._segmentLineStarts[lineIndex + 1];

        var totalWidth = 0f;
        for (int s = segStart; s < segEnd; s++)
        {
            var seg = this._segments[s];
            totalWidth += seg.IsInlineObject
                ? seg.Size.X
                : this._font!.MeasureString(line.Substring(seg.Start, seg.Length)).X;
        }

        return totalWidth;
    }

    /// <summary>
    /// Gets the number of segments in the specified line.
    /// </summary>
    /// <param name="lineIndex">The zero-based line index.</param>
    /// <returns>The number of segments.</returns>
    public int GetLineSegmentCount(int lineIndex)
    {
        this.EnsureUpToDate();
        return this._segmentLineStarts[lineIndex + 1] - this._segmentLineStarts[lineIndex];
    }

    /// <summary>
    /// Gets a specific segment within a line.
    /// </summary>
    /// <param name="lineIndex">The zero-based line index.</param>
    /// <param name="segmentIndex">The zero-based segment index within the line.</param>
    /// <returns>The inline segment.</returns>
    public InlineSegment GetLineSegment(int lineIndex, int segmentIndex)
    {
        this.EnsureUpToDate();
        return this._segments[this._segmentLineStarts[lineIndex] + segmentIndex];
    }

    /// <summary>
    /// Measures the pixel width of the first <paramref name="charCount"/> characters,
    /// accounting for inline objects and surrogate pairs.
    /// </summary>
    /// <param name="charCount">The number of characters to measure.</param>
    /// <returns>The width in pixels.</returns>
    public float MeasureChars(int charCount)
    {
        this.EnsureUpToDate();

        if (this._text.Length == 0 || charCount <= 0)
        {
            return 0f;
        }

        float maxWidth = 0f;

        for (var i = 0; i < this._lines.Count; i++)
        {
            var line = this._lines[i];
            var measureTo = Math.Min(charCount, line.Length);

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
                    width += this._font!.MeasureString(line.Substring(seg.Start, segChars)).X;
                    charsCounted += segChars;
                }
            }

            maxWidth = Math.Max(maxWidth, width);
        }

        return maxWidth;
    }

    /// <summary>
    /// Finds the inline object segment that contains the given character index, if any.
    /// </summary>
    /// <param name="charIndex">The character index to test.</param>
    /// <param name="segment">When this method returns <c>true</c>, the matching inline segment.</param>
    /// <returns><c>true</c> if the character is inside an inline object segment; otherwise <c>false</c>.</returns>
    public bool TryGetInlineSegmentAt(int charIndex, out InlineSegment segment)
    {
        this.EnsureUpToDate();
        segment = default;

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

    private void EnsureUpToDate()
    {
        if (!this._isDirty)
        {
            return;
        }

        this._isDirty = false;
        this._lines.Clear();

        // Compute actual line height.
        if (this._font != null)
        {
            this._actualLineHeight = float.IsNaN(this._lineHeight)
                ? this._font.LineHeight
                : this._lineHeight;
            this._lineSpacing = this._actualLineHeight - this._fontSize;
        }

        if (this._text.Length == 0 || this._font == null)
        {
            this._segments.Clear();
            this._segmentLineStarts.Clear();
            this._segmentLineStarts.Add(0);
            return;
        }

        switch (this._textWrap)
        {
            case TextWrap.NoWrap:
                this.SplitNoWrap();
                break;

            case TextWrap.Wrap:
                this.SplitWrap();
                break;

            default:
                throw new NotSupportedException();
        }

        this.BuildSegments();
    }

    private void SplitNoWrap()
    {
        var lineStart = 0;
        for (var i = 0; i < this._text.Length; i++)
        {
            if (this._text[i] == '\n')
            {
                this._lines.Add(this._text.AsSpan(lineStart, i - lineStart).ToString());
                lineStart = i + 1;
            }
        }

        if (lineStart < this._text.Length)
        {
            this._lines.Add(this._text.AsSpan(lineStart).ToString());
        }
    }

    private void SplitWrap()
    {
        var sb = _wrapMeasureBuilder;
        var font = this._font!;
        var maxWidth = this._availableWidth;
        var lineStart = 0;
        var lineLength = 0;
        var lastSpace = -1;

        for (var i = 0; i < this._text.Length; i++)
        {
            char c = this._text[i];
            if (c == '\n')
            {
                this._lines.Add(this._text.AsSpan(lineStart, lineLength).ToString());
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
            sb.Append(this._text.AsSpan(lineStart, Math.Min(lineLength, this._text.Length - lineStart)));
            if (font.MeasureString(sb).X > maxWidth)
            {
                if (lastSpace != -1)
                {
                    this._lines.Add(this._text.AsSpan(lineStart, lastSpace - lineStart).ToString());
                    lineStart = lastSpace + 1;
                    lineLength = i - lastSpace;
                    lastSpace = -1;
                }
                else
                {
                    this._lines.Add(this._text.AsSpan(lineStart, lineLength - 1).ToString());
                    lineStart = i;
                    lineLength = 1;
                }
            }
        }

        if (lineStart < this._text.Length)
        {
            this._lines.Add(this._text.AsSpan(lineStart).ToString());
        }
    }

    private void BuildSegments()
    {
        this._segments.Clear();
        this._segmentLineStarts.Clear();

        var provider = this._inlineObjectProvider ?? DefaultInlineObjectProvider;

        for (int i = 0; i < this._lines.Count; i++)
        {
            this._segmentLineStarts.Add(this._segments.Count);
            var line = this._lines[i];

            if (line.Length == 0)
            {
                continue;
            }

            if (provider != null)
            {
                provider.Parse(line, this._fontSize, this._parseBuffer);
                this._segments.AddRange(this._parseBuffer);
            }
            else
            {
                this._segments.Add(new InlineSegment(0, line.Length));
            }
        }

        this._segmentLineStarts.Add(this._segments.Count);
    }
}
