using System.Collections.Generic;
using System.Numerics;
using Imago.Assets.Textures;

namespace Imago.Controls;

/// <summary>
/// Represents a segment of text that is either a run of plain characters or an
/// inline content (such as an emoji sprite) to be rendered within a <see cref="TextBlock"/>.
/// </summary>
public readonly struct InlineSegment
{
    /// <summary>
    /// Gets the start index of this segment within the original text string.
    /// </summary>
    public readonly int Start;

    /// <summary>
    /// Gets the number of UTF-16 characters this segment spans in the original string.
    /// </summary>
    public readonly int Length;

    /// <summary>
    /// Gets the texture to draw for inline content segments, or <c>null</c> for text segments.
    /// </summary>
    public readonly ITextureRegion? Texture;

    /// <summary>
    /// Gets the display size in pixels for inline content segments.
    /// </summary>
    public readonly Vector2 Size;

    /// <summary>
    /// Gets a value indicating whether this segment is inline content rather than plain text.
    /// </summary>
    public bool IsInlineContent => this.Texture != null;

    /// <summary>
    /// Initializes a new text segment.
    /// </summary>
    /// <param name="start">The start index in the source string.</param>
    /// <param name="length">The number of characters in this text run.</param>
    public InlineSegment(int start, int length)
    {
        this.Start = start;
        this.Length = length;
        this.Texture = null;
        this.Size = Vector2.Zero;
    }

    /// <summary>
    /// Initializes a new inline content segment.
    /// </summary>
    /// <param name="start">The start index in the source string.</param>
    /// <param name="length">The number of characters consumed by this inline content.</param>
    /// <param name="texture">The texture region to draw.</param>
    /// <param name="size">The display size in pixels.</param>
    public InlineSegment(int start, int length, ITextureRegion texture, Vector2 size)
    {
        this.Start = start;
        this.Length = length;
        this.Texture = texture;
        this.Size = size;
    }
}

/// <summary>
/// Parses text into a sequence of <see cref="InlineSegment"/>s, identifying inline
/// content (such as emoji or icons) and determining their textures and display sizes.
/// Text blocks use this to render mixed text-and-image content.
/// </summary>
public interface IInlineContentParser
{
    /// <summary>
    /// Parses the given text into segments of plain text and inline contents.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="fontSize">The font size of the text block, for sizing context.</param>
    /// <param name="results">The list to populate with segments. The parser clears it before use.</param>
    public void Parse(string text, float fontSize, List<InlineSegment> results);
}
