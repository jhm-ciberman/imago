using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FontStashSharp;

namespace Imago.Controls;

/// <summary>
/// Provides functionality for loading fonts into FontStash font systems.
/// Results are cached so that loading the same font(s) multiple times returns
/// the same <see cref="FontSystem"/> instance.
/// </summary>
public class FontLoader
{
    private static readonly Dictionary<string, FontSystem> _cache = new();

    /// <summary>
    /// Loads fonts from the specified filesystem paths, returning a cached instance
    /// if the same set of paths has been loaded before.
    /// </summary>
    /// <param name="paths">The paths to the font files.</param>
    /// <returns>A <see cref="FontSystem"/> instance with the loaded fonts.</returns>
    public static FontSystem Load(params string[] paths)
    {
        var key = "file|" + string.Join("|", paths);

        if (_cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var fontSystem = CreateFontSystem();
        foreach (var path in paths)
        {
            fontSystem.AddFont(File.ReadAllBytes(path));
        }

        _cache[key] = fontSystem;
        return fontSystem;
    }

    /// <summary>
    /// Loads fonts from embedded manifest resources in the given assembly, returning
    /// a cached instance if the same set of resources has been loaded before.
    /// </summary>
    /// <param name="assembly">The assembly containing the embedded font resources.</param>
    /// <param name="resourceNames">The manifest resource names of the font files.</param>
    /// <returns>A <see cref="FontSystem"/> instance with the loaded fonts.</returns>
    /// <remarks>
    /// Unlike <see cref="Load"/>, this method does not touch
    /// <see cref="Visual.DefaultFontSystem"/>, so engine-internal fonts (such as the
    /// developer console font) never accidentally become the application default.
    /// </remarks>
    public static FontSystem LoadEmbedded(Assembly assembly, params string[] resourceNames)
    {
        var key = "embedded|" + assembly.FullName + "|" + string.Join("|", resourceNames);

        if (_cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var fontSystem = CreateFontSystem();
        foreach (var resourceName in resourceNames)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException(
                    $"Could not find embedded font resource '{resourceName}' in assembly '{assembly.FullName}'."
                );
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            fontSystem.AddFont(buffer.ToArray());
        }

        _cache[key] = fontSystem;
        return fontSystem;
    }

    private static FontSystem CreateFontSystem()
    {
        return new FontSystem(new FontSystemSettings
        {
            PremultiplyAlpha = false,
        });
    }
}
