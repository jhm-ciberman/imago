using System;
using System.Collections.Generic;
using System.IO;
using FontStashSharp;

namespace LifeSim.Imago;

public class Font
{
    private static readonly Dictionary<string, byte[][]> _fontSources = new();

    private record struct FontKey(string FontFamily, float Size);

    private static readonly Dictionary<FontKey, Font> _fonts = new();

    private static readonly Dictionary<string, FontSystem> _fontSystems = new(); // key: font family name, value: FontSystem

    /// <summary>
    /// Gets or sets the default font family name.
    /// </summary>
    public static string DefaultFontFamily { get; set; } = string.Empty;

    /// <summary>
    /// Loads a font from a file and registers it with the specified font family name.
    /// </summary>
    /// <param name="name">The name of the font family.</param>
    /// <param name="paths">The paths to the font files.</param>
    /// <exception cref="InvalidOperationException">Thrown if the font family name is already registered.</exception>
    public static void LoadFont(string name, params string[] paths)
    {
        if (!TryLoadFont(name, paths))
            throw new InvalidOperationException($"Font '{name}' is already loaded.");
    }

    /// <summary>
    /// Loads a font from a file and registers it with the specified font family name only if it is not already loaded.
    /// </summary>
    /// <param name="name">The name of the font family.</param>
    /// <param name="paths">The paths to the font files.</param>
    /// <returns>True if the font was loaded, false otherwise.</returns>
    public static bool TryLoadFont(string name, params string[] paths)
    {
        if (IsFontLoaded(name))
            return false;

        var fontBytes = new byte[paths.Length][];
        for (var i = 0; i < paths.Length; i++)
        {
            var path = paths[i];
            fontBytes[i] = File.ReadAllBytes(path);
        }

        _fontSources.Add(name, fontBytes);

        if (DefaultFontFamily == "")
            DefaultFontFamily = name;

        return true;
    }

    /// <summary>
    /// Gets whether the specified font family is loaded.
    /// </summary>
    /// <param name="fontFamily">The name of the font family.</param>
    /// <returns>true if the font family is loaded; otherwise, false.</returns>
    public static bool IsFontLoaded(string fontFamily)
    {
        return _fontSources.ContainsKey(fontFamily);
    }

    /// <summary>
    /// Gets a font with the specified font family name and size.
    /// </summary>
    /// <param name="fontFamily">The name of the font family.</param>
    /// <param name="fontSize">The size of the font.</param>
    /// <returns>The font.</returns>
    public static Font GetFont(string? fontFamily, float fontSize)
    {
        return GetFontCore(fontFamily, fontSize);
    }

    private static Font GetFontCore(string? fontFamily, float size)
    {
        var key = new FontKey(fontFamily ?? DefaultFontFamily, size);
        if (!_fonts.TryGetValue(key, out var font))
        {
            font = new Font(key.FontFamily, key.Size);
            _fonts.Add(key, font);
        }
        return font;
    }

    /// <summary>
    /// Gets the font family name.
    /// </summary>
    public string FontFamily { get; }

    /// <summary>
    /// Gets the font size.
    /// </summary>
    public float FontSize => this.FontBase.FontSize;

    /// <summary>
    /// Gets the line height.
    /// </summary>
    public int LineHeight => this.FontBase.LineHeight;

    /// <summary>
    /// Gets the internal font base used by the render system.
    /// </summary>
    internal SpriteFontBase FontBase { get; }

    private Font(string fontFamily, float fontSize)
    {
        this.FontFamily = fontFamily;

        // First try to search for a font system with the same font family and effect.
        if (_fontSystems.TryGetValue(fontFamily, out FontSystem? fontSystem))
        {
            this.FontBase = fontSystem.GetFont(fontSize);
            return;
        }

        // If not found, create a new font system.
        if (!IsFontLoaded(fontFamily))
            throw new ArgumentException($"Font {fontFamily} not loaded.");

        fontSystem = new FontSystem(new FontSystemSettings
        {
            PremultiplyAlpha = false,
        });
        var dataArr = _fontSources[fontFamily];

        for (var i = 0; i < dataArr.Length; i++)
        {
            fontSystem.AddFont(dataArr[i]);
        }

        _fontSystems.Add(fontFamily, fontSystem); // Add it to the cache.

        this.FontBase = fontSystem.GetFont(fontSize);
    }
}
