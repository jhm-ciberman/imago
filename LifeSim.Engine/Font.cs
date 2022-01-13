using System.IO;
using FontStashSharp;
using FontStashSharp.Interfaces;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine;

public class Font
{
    private readonly FontSystem _fontSystem;

    public Font(string[] paths)
    {
        var fontLoader = StbTrueTypeSharpFontLoader.Instance;
        var textureManager = Renderer.Instance;
        int atlasSize = 1024;
        this._fontSystem = new FontSystem(fontLoader, textureManager, atlasSize, atlasSize, 0, 1, true);
        foreach (var path in paths)
        {
            this._fontSystem.AddFont(File.ReadAllBytes(path));
        }
    }

    public DynamicSpriteFont GetFont(int size)
    {
        return this._fontSystem.GetFont(size);
    }
}