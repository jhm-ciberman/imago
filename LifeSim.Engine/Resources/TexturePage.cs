using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LifeSim.Engine.Resources;

/// <summary>
/// The TexturePage class is a texture atlas. It is used to pack textures into a single texture.
/// </summary>
public class TexturePage
{
    private const float TEXEL_EPSILON = 0f; //0.05f; // 1/20th of a texel 

    private readonly uint _tileSize;
    private readonly BinPacker _binPacker;

    /// <summary>
    /// Whether the atlas texture is dirty and needs to be updated.
    /// </summary>
    public bool IsDirty { get; private set; } = false;

    /// <summary>
    /// The image that will be used to store the atlas.
    /// </summary>
    public Image<Rgba32> Image { get; }

    /// <summary>
    /// The texture that contains the atlas.
    /// </summary>
    public Texture Texture { get; }

    /// <summary>
    /// Gets the group that this page belongs to.
    /// </summary>
    public TextureGroup Group { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TexturePage"/> class.
    /// </summary>
    /// <param name="group">The group that the page belongs to.</param>
    public TexturePage(TextureGroup group)
    {
        this.Group = group;
        uint tileSize = NextPowOfTwo(group.TileSize);
        uint atlasSize = (uint)1 << BitOperations.Log2(group.AtlasSize);
        uint mipMapLevels = (uint)BitOperations.Log2(tileSize);

        uint size = atlasSize / tileSize;
        this._binPacker = new BinPacker(size, size);
        this._tileSize = tileSize;

        this.Image = new Image<Rgba32>((int)atlasSize, (int)atlasSize);
        this.Texture = new Texture((uint)this.Image.Width, (uint)this.Image.Height, mipMapLevels, group.IsSrgb);
        this.Texture.Name = $"Texture Page ({group.Name})";
    }

    /// <summary>
    /// Apply the changes to the texture.
    /// </summary>
    public void Apply()
    {
        if (this.IsDirty)
        {
            this.Texture.SetDataFromImage(this.Image);
            this.IsDirty = false;
        }
    }

    /// <summary>
    /// Try to add an element to the atlas. Returns a result object containing the UV coordinates of the element.
    /// The method returns false if the element cannot be added to the atlas.
    /// </summary>
    /// <param name="operation">The element to add.</param>
    /// <param name="result">The result object containing the UV coordinates of the element.</param>
    /// <returns>True if the element was added to the atlas, false otherwise.</returns>
    public bool TryPack(IDrawOperation operation, [MaybeNullWhen(false)] out PackedTexture result)
    {
        if (this._binPacker.IsFull) // Early out if the atlas is full
        {
            result = default;
            return false;
        }

        uint w, h;
        if (this._tileSize == 0)
        {
            w = (uint)operation.Size.X;
            h = (uint)operation.Size.Y;
        }
        else
        {
            w = (uint)MathF.Ceiling(operation.Size.X / (float)this._tileSize);
            h = (uint)MathF.Ceiling(operation.Size.Y / (float)this._tileSize);
        }

        if (!this._binPacker.TryFit(w, h, out Vector2Int coords))
        {
            result = default;
            return false;
        }

        if (this._tileSize != 0)
        {
            coords *= this._tileSize;
        }

        operation.Draw(this.Image, coords);
        this.IsDirty = true;

        Vector2 imgSize = new Vector2(this.Image.Width, this.Image.Height);
        Vector2 uvTopLeft = coords / imgSize;
        Vector2 uvBottomRight = (coords + operation.Size) / imgSize;

        var texelEpsilon = Vector2.One / imgSize * TEXEL_EPSILON;
        result = new PackedTexture(this.Texture, uvTopLeft - texelEpsilon, uvBottomRight + texelEpsilon);

        return true;
    }

    private static uint NextPowOfTwo(uint x)
    {
        --x;
        x |= x >> 1;
        x |= x >> 2;
        x |= x >> 4;
        x |= x >> 8;
        x |= x >> 16;
        return x + 1;
    }
}