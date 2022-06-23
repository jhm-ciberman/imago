using System;
using System.Collections.Generic;
using System.IO;
using FontStashSharp;

namespace LifeSim.Engine.Rendering;

public class Font
{
    private static readonly Dictionary<string, byte[][]> _fontSources = new();

    private record struct FontKey(string FontFamily, int Size, FontSystemEffect Effect, int EffectAmount);

    private record struct FontSystemKey(string FontFamily, FontSystemEffect Effect, int EffectAmount);

    private static readonly Dictionary<FontKey, Font> _fonts = new();

    private static readonly Dictionary<FontSystemKey, FontSystem> _fontSystems = new();

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
        {
            throw new InvalidOperationException($"Font '{name}' is already loaded.");
        }
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
        {
            return false;
        }

        var fontBytes = new byte[paths.Length][];
        for (var i = 0; i < paths.Length; i++)
        {
            var path = paths[i];
            fontBytes[i] = File.ReadAllBytes(path);
        }

        _fontSources.Add(name, fontBytes);

        if (DefaultFontFamily == "")
        {
            DefaultFontFamily = name;
        }

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
    /// Gets a blurred font with the specified font family name and size and blur amount.
    /// </summary>
    /// <param name="fontFamily">The name of the font family.</param>
    /// <param name="size">The size of the font.</param>
    /// <param name="blurAmount">The blur amount of the font.</param>
    /// <returns>The font.</returns>
    public static Font GetBlurredFont(string? fontFamily, int fontSize, int blur)
    {
        return (blur == 0)
            ? GetFontCore(fontFamily, fontSize, FontSystemEffect.None, 0)
            : GetFontCore(fontFamily, fontSize, FontSystemEffect.Blurry, blur);
    }

    /// <summary>
    /// Gets a stroked font with the specified font family name and size and stroke amount.
    /// </summary>
    /// <param name="fontFamily">The name of the font family.</param>
    /// <param name="size">The size of the font.</param>
    /// <param name="strokeAmount">The stroke amount of the font.</param>
    /// <returns>The font.</returns>
    public static Font GetStrokedFont(string? fontFamily, int fontSize, int stroke)
    {
        return (stroke == 0)
            ? GetFontCore(fontFamily, fontSize, FontSystemEffect.None, 0)
            : GetFontCore(fontFamily, fontSize, FontSystemEffect.Stroked, stroke);
    }

    /// <summary>
    /// Gets a font with the specified font family name and size.
    /// </summary>
    /// <param name="fontFamily">The name of the font family.</param>
    /// <param name="size">The size of the font.</param>
    /// <returns>The font.</returns>
    public static Font GetFont(string? fontFamily, int fontSize)
    {
        return GetFontCore(fontFamily, fontSize, FontSystemEffect.None, 0);
    }

    private static Font GetFontCore(string? fontFamily, int size, FontSystemEffect effect, int effectAmount)
    {
        var key = new FontKey(fontFamily ?? DefaultFontFamily, size, effect, effectAmount);
        if (!_fonts.TryGetValue(key, out var font))
        {
            font = new Font(key.FontFamily, key.Size, key.Effect, key.EffectAmount);
            _fonts.Add(key, font);
        }
        return font;
    }


    #region Font class
    /// <summary>
    /// Gets the font family name.
    /// </summary>
    public string FontFamily { get; }

    /// <summary>
    /// Gets the font size.
    /// </summary>
    public int FontSize => this.FontBase.FontSize;

    /// <summary>
    /// Gets the line height.
    /// </summary>
    public int LineHeight => this.FontBase.LineHeight;

    /// <summary>
    /// Gets the internal font base used by the render system.
    /// </summary>
    internal SpriteFontBase FontBase { get; }

    private Font(string fontFamily, int fontSize, FontSystemEffect effect, int effectAmount)
    {
        this.FontFamily = fontFamily;

        // First try to search for a font system with the same font family and effect.
        var key = new FontSystemKey(fontFamily, effect, effectAmount);
        if (_fontSystems.TryGetValue(key, out FontSystem? fontSystem))
        {
            this.FontBase = fontSystem.GetFont(fontSize);
            return;
        }

        // If not found, create a new font system.
        if (!IsFontLoaded(fontFamily))
        {
            throw new ArgumentException($"Font {fontFamily} not loaded.");
        }

        fontSystem = new FontSystem(new FontSystemSettings
        {
            Effect = effect,
            EffectAmount = effectAmount,
            PremultiplyAlpha = false,
        });
        var dataArr = _fontSources[fontFamily];

        for (var i = 0; i < dataArr.Length; i++)
        {
            fontSystem.AddFont(dataArr[i]);
        }

        _fontSystems.Add(key, fontSystem); // Add it to the cache.

        this.FontBase = fontSystem.GetFont(fontSize);
    }

    #endregion
}