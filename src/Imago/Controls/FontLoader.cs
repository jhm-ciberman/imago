using System.Collections.Generic;
using System.IO;
using FontStashSharp;

namespace Imago.Controls;

/// <summary>
/// Provides functionality for loading fonts into FontStash font systems.
/// Results are cached by path so that loading the same font(s) multiple times
/// returns the same <see cref="FontSystem"/> instance.
/// </summary>
public class FontLoader
{
    private static readonly Dictionary<string, FontSystem> _cache = new();

    /// <summary>
    /// Loads fonts from the specified paths, returning a cached instance if the
    /// same set of paths has been loaded before.
    /// </summary>
    /// <param name="paths">The paths to the font files.</param>
    /// <returns>A <see cref="FontSystem"/> instance with the loaded fonts.</returns>
    public static FontSystem Load(params string[] paths)
    {
        var key = string.Join("|", paths);

        if (_cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var fontSystem = new FontSystem(new FontSystemSettings
        {
            PremultiplyAlpha = false,
        });

        foreach (var path in paths)
        {
            fontSystem.AddFont(File.ReadAllBytes(path));
        }

        Visual.DefaultFontSystem ??= fontSystem;
        _cache[key] = fontSystem;
        return fontSystem;
    }
}
