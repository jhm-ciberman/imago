using System;
using System.Collections.Generic;
using System.IO;
using FontStashSharp;
using FontStashSharp.Interfaces;

namespace LifeSim.Engine.Rendering;

public static class FontManager
{
    private static readonly Dictionary<string, byte[][]> _fontSources = new();

    private record struct FontKey(string FontFamily, int Size, FontSystemEffect Effect, int EffectAmount);

    private record struct FontSystemKey(string FontFamily, FontSystemEffect Effect, int EffectAmount);

    private static readonly Dictionary<FontKey, SpriteFontBase> _fonts = new();

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
    /// <param name="name">The name of the font family.</param>
    /// <returns>true if the font family is loaded; otherwise, false.</returns>
    public static bool IsFontLoaded(string name)
    {
        return _fontSources.ContainsKey(name);
    }

    private static SpriteFontBase GetFontCore(string? fontFamily, int size, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
    {
        var key = new FontKey(fontFamily ?? DefaultFontFamily, size, effect, effectAmount);
        if (!_fonts.TryGetValue(key, out var font))
        {
            font = MakeFont(key);
            _fonts.Add(key, font);
        }
        return font;
    }

    /// <summary>
    /// Gets a blurred font with the specified font family name and size and blur amount.
    /// </summary>
    /// <param name="fontFamily">The name of the font family.</param>
    /// <param name="size">The size of the font.</param>
    /// <param name="blurAmount">The blur amount of the font.</param>
    /// <returns>The font.</returns>
    public static SpriteFontBase GetBlurredFont(string? fontFamily, int fontSize, int blur)
    {
        return GetFontCore(fontFamily, fontSize, FontSystemEffect.Blurry, blur);
    }

    /// <summary>
    /// Gets a stroked font with the specified font family name and size and stroke amount.
    /// </summary>
    /// <param name="fontFamily">The name of the font family.</param>
    /// <param name="size">The size of the font.</param>
    /// <param name="strokeAmount">The stroke amount of the font.</param>
    /// <returns>The font.</returns>
    public static SpriteFontBase GetStrokedFont(string? fontFamily, int fontSize, int stroke)
    {
        return GetFontCore(fontFamily, fontSize, FontSystemEffect.Stroked, stroke);
    }

    /// <summary>
    /// Gets a font with the specified font family name and size.
    /// </summary>
    /// <param name="fontFamily">The name of the font family.</param>
    /// <param name="size">The size of the font.</param>
    /// <returns>The font.</returns>
    public static SpriteFontBase GetFont(string? fontFamily, int fontSize)
    {
        return GetFontCore(fontFamily, fontSize);
    }

    private static SpriteFontBase MakeFont(FontKey fontKey)
    {
        // First try to search for a font system with the same font family and effect.
        var key = new FontSystemKey(fontKey.FontFamily, fontKey.Effect, fontKey.EffectAmount);
        if (_fontSystems.TryGetValue(key, out FontSystem? fontSystem))
        {
            return fontSystem.GetFont(fontKey.Size);
        }

        // If not found, create a new font system.
        if (!IsFontLoaded(fontKey.FontFamily))
        {
            throw new ArgumentException($"Font {fontKey.FontFamily} not loaded.");
        }

        fontSystem = new FontSystem(new FontSystemSettings
        {
            Effect = fontKey.Effect,
            EffectAmount = fontKey.EffectAmount,
        });
        var dataArr = _fontSources[fontKey.FontFamily];

        for (var i = 0; i < dataArr.Length; i++)
        {
            fontSystem.AddFont(dataArr[i]);
        }

        _fontSystems.Add(key, fontSystem); // Add it to the cache.

        return fontSystem.GetFont(fontKey.Size);
    }
}