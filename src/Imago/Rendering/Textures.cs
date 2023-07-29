using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Imago.Rendering;

public static class Textures
{
    private static Texture? _white = null;
    private static Texture? _black = null;
    private static Texture? _transparent = null;
    private static Texture? _magenta = null;


    /// <summary>
    /// Gets a small white texture.
    /// </summary>
    public static Texture White => _white ??= new ImageTexture(new Image<Rgba32>(2, 2, new Rgba32(255, 255, 255, 255)));

    /// <summary>
    /// Gets a small black texture.
    /// </summary>
    public static Texture Black => _black ??= new ImageTexture(new Image<Rgba32>(2, 2, new Rgba32(0, 0, 0, 255)));

    /// <summary>
    /// Gets a small transparent texture.
    /// </summary>
    public static Texture Transparent => _transparent ??= new ImageTexture(new Image<Rgba32>(2, 2, new Rgba32(0, 0, 0, 0)));

    /// <summary>
    /// Gets a small magenta texture.
    /// </summary>
    public static Texture Magenta => _magenta ??= new ImageTexture(new Image<Rgba32>(2, 2, new Rgba32(255, 0, 255, 255)));
}
