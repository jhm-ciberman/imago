using System;
using System.Collections.Generic;
using System.IO;
using FontStashSharp;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Resources;

public static class Font
{
    private struct FontKey
    {
        public string FontFamily { get; }
        public int Outline { get; }

        public int Blur { get; }

        public FontKey(string name, int outline, int blur)
        {
            this.FontFamily = name;
            this.Outline = outline;
            this.Blur = blur;
        }
    }

    public struct FontData
    {
        public byte[] Data { get; set; }
    }

    private static readonly Dictionary<FontKey, FontSystem> _fontSystems = new Dictionary<FontKey, FontSystem>();

    private static readonly Dictionary<string, FontData[]> _fontsBytes = new Dictionary<string, FontData[]>();

    private static string _fallbackFontFamily = "";

    public static void LoadFont(string name, params string[] paths)
    {
        var fontBytes = new FontData[paths.Length];
        for (var i = 0; i < paths.Length; i++)
        {
            var path = paths[i];
            var bytes = File.ReadAllBytes(path);
            fontBytes[i] = new FontData { Data = bytes };
        }

        _fontsBytes.Add(name, fontBytes);

        if (_fallbackFontFamily == "")
        {
            _fallbackFontFamily = name;
        }
    }

    public static void SetFallbackFont(string name)
    {
        _fallbackFontFamily = name;
    }


    public static DynamicSpriteFont GetFont(string? fontFamily, int size, int outline = 0, int blur = 0)
    {
        var key = new FontKey(fontFamily ?? _fallbackFontFamily, outline, blur);
        if (!_fontSystems.TryGetValue(key, out var fontSystem))
        {
            fontSystem = MakeSystem(key);
        }
        return fontSystem.GetFont(size);
    }

    private static FontSystem MakeSystem(FontKey fontKey)
    {
        if (!_fontsBytes.ContainsKey(fontKey.FontFamily))
        {
            throw new ArgumentException($"Font {fontKey.FontFamily} not loaded.");
        }

        int atlasSize = 1024;
        var fontSystem = new FontSystem(Renderer.Instance, atlasSize, atlasSize, fontKey.Blur, fontKey.Outline, false);
        var dataArr = _fontsBytes[fontKey.FontFamily];

        for (var i = 0; i < dataArr.Length; i++)
        {
            fontSystem.AddFont(dataArr[i].Data);
        }

        _fontSystems.Add(fontKey, fontSystem);
        return fontSystem;
    }



}