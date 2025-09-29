using System.IO;
using FontStashSharp;
using LifeSim.Imago.Controls;

namespace LifeSim.Imago;

/// <summary>
/// Provides functionality for loading fonts into FontStash font systems.
/// </summary>
public class FontLoader
{
    /// <summary>
    /// Helper method to load fonts from the specified paths.
    /// </summary>
    /// <param name="paths">The paths to the font files.</param>
    /// <returns>A <see cref="FontSystem"/> instance with the loaded fonts.</returns>
    public static FontSystem Load(params string[] paths)
    {
        var fontSystem = new FontSystem(new FontSystemSettings
        {
            PremultiplyAlpha = false,
        });

        foreach (var path in paths)
        {
            fontSystem.AddFont(File.ReadAllBytes(path));
        }

        Visual.DefaultFontSystem ??= fontSystem;
        return fontSystem;
    }
}
