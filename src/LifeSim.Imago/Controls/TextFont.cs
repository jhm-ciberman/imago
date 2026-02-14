using FontStashSharp;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Bundles a font family with its intended pixel size and optional line height.
/// Use this to define a font once and reference it by a single object,
/// ensuring the correct size is always applied.
/// </summary>
/// <param name="system">The font system (family) to use.</param>
/// <param name="size">The font size in pixels.</param>
/// <param name="lineHeight">The line height in pixels, or <see cref="float.NaN"/> to use the font default.</param>
public class TextFont(FontSystem system, float size, float lineHeight = float.NaN)
{
    /// <summary>
    /// Gets the font system (family).
    /// </summary>
    public FontSystem System { get; } = system;

    /// <summary>
    /// Gets the font size in pixels.
    /// </summary>
    public float Size { get; } = size;

    /// <summary>
    /// Gets the line height in pixels, or <see cref="float.NaN"/> to use the font default.
    /// </summary>
    public float LineHeight { get; } = lineHeight;
}
