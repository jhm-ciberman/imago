using System;
using System.Collections.Generic;

namespace LifeSim.Engine.Resources;

public class TexturePacker
{
    /// <summary>
    /// Event that is raised when a new page is added to the packer.
    /// </summary>
    public event EventHandler<TexturePage>? PageAdded;

    /// <summary>
    /// Gets a list of all the pages in the texture manager.
    /// </summary>
    public IReadOnlyList<TexturePage> Pages => this._pages;

    /// <summary>
    /// Gets the size of each page in the texture manager in pixels.
    /// </summary>
    public uint AtlasSize { get; }

    /// <summary>
    /// Gets the size of each tile in the texture manager in pixels.
    public uint TileSize { get; }

    private readonly List<TexturePage> _pages = new(1);

    /// <summary>
    /// Initializes a new instance of the <see cref="TexturePacker"/> class.
    /// </summary>
    /// <param name="atlasSize">The size of each page in the texture manager in pixels.</param>
    /// <param name="tileSize">The size of each tile in the texture manager in pixels.</param>
    public TexturePacker(uint atlasSize, uint tileSize)
    {
        this.AtlasSize = atlasSize;
        this.TileSize = tileSize;
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
                return packedTexture;
            }
        }

        // If any page can't fit the request, we will need to create a new page and fit the request.
        var newPage = new TexturePage(this.AtlasSize, this.TileSize);
        this._pages.Add(newPage);
        this.PageAdded?.Invoke(this, newPage);
        if (newPage.TryPack(unpackedTexture, out packedTexture))
        {
            return packedTexture;
        }

        // If the request cannot fit even in the newly empty page, then it's a fatal error
        // probably because the request is too big.
        throw new InvalidOperationException("Cannot pack texture in any texture page");
    }

    /// <summary>
    /// Flushes all the changes made to the texture packer.
    /// </summary>
    public void FlushChanges()
    {
        foreach (var page in this._pages)
        {
            page.Apply();
        }
    }
}