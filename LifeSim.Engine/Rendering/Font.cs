using System;
using System.Collections.Generic;
using System.IO;
using FontStashSharp;
using FontStashSharp.Interfaces;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Rendering;

public static class Font
{
    public record struct Key(string FontFamily, int Outline, int Blur);

    public record struct FontData(byte[] Data);

    private static readonly Dictionary<Key, FontSystem> _fontSystems = new Dictionary<Key, FontSystem>();

    private static readonly Dictionary<string, FontData[]> _fontsBytes = new Dictionary<string, FontData[]>();

    /// <summary>
    /// Gets or sets the default font family name. 
    /// </summary>
    public static string DefaultFontFamily { get; set; } = string.Empty;

    /// <summary>
    /// Loads a font from a file and registers it with the specified font family name.
    /// </summary>
    /// <param name="name">The name of the font family.</param>
    /// <param name="paths">The paths to the font files.</param>
    public static void LoadFont(string name, params string[] paths)
    {
        if (IsFontLoaded(name))
        {
            throw new InvalidOperationException($"Font '{name}' is already loaded.");
        }

        LoadFontCore(name, paths);
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

        LoadFontCore(name, paths);
        return true;
    }

    private static void LoadFontCore(string name, string[] paths)
    {
        var fontBytes = new FontData[paths.Length];
        for (var i = 0; i < paths.Length; i++)
        {
            var path = paths[i];
            var bytes = File.ReadAllBytes(path);
            fontBytes[i] = new FontData { Data = bytes };
        }

        _fontsBytes.Add(name, fontBytes);

        if (DefaultFontFamily == "")
        {
            DefaultFontFamily = name;
        }
    }

    /// <summary>
    /// Gets whether the specified font family is loaded.
    /// </summary>
    /// <param name="name">The name of the font family.</param>
    /// <returns>true if the font family is loaded; otherwise, false.</returns>
    public static bool IsFontLoaded(string name)
    {
        return _fontsBytes.ContainsKey(name);
    }


    public static DynamicSpriteFont GetFont(string? fontFamily, int size, int outline = 0, int blur = 0)
    {
        var key = new Key(fontFamily ?? DefaultFontFamily, outline, blur);
        if (!_fontSystems.TryGetValue(key, out var fontSystem))
        {
            fontSystem = MakeSystem(key);
        }
        return fontSystem.GetFont(size);
    }

    private static FontSystem MakeSystem(Key fontKey)
    {
        if (!_fontsBytes.ContainsKey(fontKey.FontFamily))
        {
            throw new ArgumentException($"Font {fontKey.FontFamily} not loaded.");
        }

        var fontSystem = new FontSystem();
        var dataArr = _fontsBytes[fontKey.FontFamily];

        for (var i = 0; i < dataArr.Length; i++)
        {
            fontSystem.AddFont(dataArr[i].Data);
        }

        _fontSystems.Add(fontKey, fontSystem);
        return fontSystem;
    }
}