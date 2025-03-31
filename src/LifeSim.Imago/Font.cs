using System.IO;
using FontStashSharp;

namespace LifeSim.Imago;

public class FontLoader
{
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
