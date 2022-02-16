using System;
using System.IO;
using FontStashSharp;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Resources;

public class Font : IDisposable
{
    private readonly FontSystem _fontSystem;

    private static Font? _defaultFont;
    public static Font Default
    {
        get
        {
            if (_defaultFont == null)
            {
                _defaultFont = new Font(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"res/fonts/DroidSans.ttf"));
            }

            return _defaultFont;
        }

        set => _defaultFont = value;
    }

    public Font(string path) : this(new string[] { path })
    {
    }

    public Font(string[] paths)
    {
        var fontLoader = StbTrueTypeSharpFontLoader.Instance;
        int atlasSize = 1024;
        this._fontSystem = new FontSystem(fontLoader, Renderer.Instance, atlasSize, atlasSize, 0, 1, true);
        foreach (var path in paths)
        {
            this._fontSystem.AddFont(File.ReadAllBytes(path));
        }
    }

    public void Dispose()
    {
        ((IDisposable)this._fontSystem).Dispose();
    }

    public DynamicSpriteFont GetFont(int size)
    {
        return this._fontSystem.GetFont(size);
    }
}