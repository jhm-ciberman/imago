using System;
using LifeSim.Engine.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LifeSim.Engine.Resources;

/// <summary>
/// Creates high level resources for the renderer.
/// </summary>
public class RendererResourceFactory
{
    private readonly Renderer _renderer;

    internal RendererResourceFactory(Renderer renderer)
    {
        this._renderer = renderer;
    }

    public Font CreateFont(string[] paths)
    {
        return new Font(this._renderer, paths);
    }

    public Texture CreateTexture(uint width, uint height, uint mipLevels = 0)
    {
        return new Texture(this._renderer, width, height, mipLevels);
    }

    public Texture CreateTexture(string path, bool srgb = true)
    {
        using var img = Image.Load<Rgba32>(path);
        return this.CreateTexture(img, srgb);
    }

    public Texture CreateTexture(Image<Rgba32> image, bool srgb = true)
    {
        var texture = new Texture(this._renderer, (uint)image.Width, (uint)image.Height, 0, srgb);
        texture.SetDataFromImage(image);
        return texture;
    }

    public Mesh CreateMesh(IMeshData meshData)
    {
        return new Mesh(this._renderer, meshData);
    }
}