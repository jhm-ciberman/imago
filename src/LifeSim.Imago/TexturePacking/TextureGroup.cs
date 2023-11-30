using System;
using System.Collections.Generic;

namespace LifeSim.Imago.TexturePacking;

/// <summary>
/// A texture group is a container for one or more <see cref="TexturePage"/>s. Each texture group
/// defines the size of the atlas that will be used to pack the textures as well as the alignment of
/// the items inside the atlas.
///
/// Each group has a name that can be used for finding the group in the <see cref="TexturePacker"/>.
/// It is recomended to group the textures according to the type of textures. For example: UI, Terrain, game objects, etc.
/// </summary>
public class TextureGroup : IDisposable
{
    /// <summary>
    /// Occurs when a new page is added to the group.
    /// </summary>
    public event EventHandler<TexturePage>? PageAdded;

    /// <summary>
    /// Occurs when the group needs to flush the pending changes to the atlas.
    /// </summary>
    public event EventHandler? FlushRequested;

    /// <summary>
    /// Gets the name of the group.
    /// </summary>
    public string Name { get; } = string.Empty;

    /// <summary>
    /// Gets the size of the atlas.
    /// </summary>
    public uint AtlasSize { get; } = 0;

    /// <summary>
    /// Gets the tile size. This is the size in pixels of each tile in the atlas. This can be used to prevent MipMap levels
    /// from bleeding into each other.
    /// </summary>
    public uint TileSize { get; } = 1;

    /// <summary>
    /// Gets whether the texture pages of this group should use the SRGB color space.
    /// </summary>
    public bool IsSrgb { get; } = false;

    private readonly List<TexturePage> _pages = new(1);

    private readonly Dictionary<PackedTexture, TexturePage> _packedTexturesPages = new();

    private bool _flushRequested = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextureGroup"/> class.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <param name="atlasSize">The size of the atlas.</param>
    /// <param name="tileSize">The tile size.</param>
    /// <param name="srgb">Whether the texture pages of this group should use the SRGB color space.</param>
    public TextureGroup(string name, uint atlasSize, uint tileSize, bool srgb = false)
    {
        this.Name = name;
        this.AtlasSize = atlasSize;
        this.TileSize = tileSize;
        this.IsSrgb = srgb;
    }

    /// <summary>
    /// Packs the specified texture and returns a packed texture.
    /// </summary>
    /// <param name="unpackedTexture">The texture to pack.</param>
    /// <returns>The packed texture.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the texture is already packed or it's too big to be packed in any page.</exception>
    public PackedTexture Pack(IDrawOperation unpackedTexture)
    {
        PackedTexture? packedTexture;

        // Try to find any already allocated page that can fit the request
        foreach (var currentPage in this._pages)
        {
            if (currentPage.TryPack(unpackedTexture, out packedTexture))
            {
                this._packedTexturesPages.Add(packedTexture, currentPage);
                this.RequestFlush();
                return packedTexture;
            }
        }

        // If any page can't fit the request, we will need to create a new page and fit the request.
        var page = new TexturePage(this);
        this._pages.Add(page);
        this.PageAdded?.Invoke(this, page);
        if (page.TryPack(unpackedTexture, out packedTexture))
        {
            this._packedTexturesPages.Add(packedTexture, page);
            this.RequestFlush();
            return packedTexture;
        }

        // If the request cannot fit even in the newly empty page, then it's a fatal error
        // probably because the request is too big.
        throw new InvalidOperationException("Cannot pack texture in any texture page");
    }

    /// <summary>
    /// Packs the specified texture and returns a packed texture.
    /// </summary>
    /// <param name="sourcePath">The path to the texture to pack.</param>
    /// <returns>The packed texture.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the texture is already packed or it's too big to be packed in any page.</exception>
    public PackedTexture Pack(string sourcePath)
    {
        return this.Pack(new TextureDrawOperation(sourcePath));
    }

    protected void RequestFlush()
    {
        if (!this._flushRequested)
        {
            this._flushRequested = true;
            this.FlushRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets a list of all the pages in the group.
    /// </summary>
    public IReadOnlyList<TexturePage> Pages => this._pages;

    /// <summary>
    /// Flushes the pending changes to the atlas.
    /// </summary>
    public void FlushChanges()
    {
        if (!this._flushRequested) return;
        this._flushRequested = false;

        foreach (var currentPage in this._pages)
        {
            currentPage.Apply();
        }
    }

    /// <summary>
    /// Releases the specified packed texture.
    /// </summary>
    /// <param name="packedTexture">The packed texture to release.</param>
    /// <exception cref="InvalidOperationException">Thrown when the texture is not found in any page.</exception>
    public void Release(PackedTexture packedTexture)
    {
        if (!this._packedTexturesPages.TryGetValue(packedTexture, out var page))
        {
            throw new InvalidOperationException("Texture not found in any page.");
        }

        page.Release(packedTexture);
        this._packedTexturesPages.Remove(packedTexture);

        // No need to flush changes. We will keep the old texture pixels
        // in the atlas until it's overwritten by a new texture.
    }


    /// <summary>
    /// Redraws the texture contained in the specified packed texture. This is useful when you want to update the texture
    /// without having to release and repack it.
    /// </summary>
    /// <param name="packedTexture">The packed texture to redraw.</param>
    /// <param name="drawOperation">The draw operation to use.</param>
    /// <exception cref="InvalidOperationException">Thrown when the texture is not found in any page.</exception>
    public void Redraw(PackedTexture packedTexture, IDrawOperation drawOperation)
    {
        if (!this._packedTexturesPages.TryGetValue(packedTexture, out var page))
        {
            throw new InvalidOperationException("Texture not found in any page.");
        }

        page.Redraw(packedTexture, drawOperation);
        this.RequestFlush();
    }

    /// <summary>
    /// Releases all resources used by the group.
    /// </summary>
    public void Dispose()
    {
        foreach (var page in this._pages)
        {
            page.Dispose();
        }
    }
}
