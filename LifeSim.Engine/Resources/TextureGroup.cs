using System;
using System.Collections.Generic;

namespace LifeSim.Engine.Resources;

/// <summary>
/// A texture group is a container for one or more <see cref="TexturePage"/>s. Each texture group
/// defines the size of the atlas that will be used to pack the textures as well as the alignment of
/// the items inside the atlas. 
/// 
/// Each group has a name that can be used for finding the group in the <see cref="TexturePacker"/>.
/// It is recomended to group the textures according to the type of textures. For example: UI, Terrain, game objects, etc.
/// </summary>
public class TextureGroup
{
    /// <summary>
    /// Event that is raised when a new page is added to the group.
    /// </summary>
    public event EventHandler<TexturePage>? PageAdded;

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
    /// from bleeding into each other. A size of 0 means that the texture will not be tiled.
    /// </summary>
    public uint TileSize { get; } = 0;

    /// <summary>
    /// Gets whether the texture pages of this group should use the SRGB color space.
    /// </summary>
    public bool IsSrgb { get; } = false;

    private readonly List<TexturePage> _pages = new(1);

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
                return packedTexture;
            }
        }

        // If any page can't fit the request, we will need to create a new page and fit the request.
        var page = new TexturePage(this);
        this._pages.Add(page);
        this.PageAdded?.Invoke(this, page);
        if (page.TryPack(unpackedTexture, out packedTexture))
        {
            return packedTexture;
        }

        // If the request cannot fit even in the newly empty page, then it's a fatal error
        // probably because the request is too big.
        throw new InvalidOperationException("Cannot pack texture in any texture page");
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
        foreach (var currentPage in this._pages)
        {
            currentPage.Apply();
        }
    }
}
