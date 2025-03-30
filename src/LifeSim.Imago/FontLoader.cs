using System.IO;
using FontStashSharp;

namespace LifeSim.Imago;

public class FontLoader
{
    /// <summary>
    /// Helper method to load fonts from the specified paths.
    /// </summary>
    /// <param name="paths">The paths to the font files.</param>
    /// <returns>A <see cref="FontSystem"/> instance with the loaded fonts.</returns>
    public static FontSystem Load(params string[] paths)
    {
        var fontSystem = new FontSystem();
        foreach (var path in paths)
        {
            fontSystem.AddFont(File.ReadAllBytes(path));
        }
        return fontSystem;
    }
}
