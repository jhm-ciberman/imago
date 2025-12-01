using System;
using System.Collections.Generic;
using System.IO;
using LifeSim.Support;

namespace LifeSim.Imago.TexturePacking;

/// <summary>
/// Manages a collection of texture atlases (<see cref="TexturePage"/>s) for packing individual textures into larger sheets.
/// </summary>
public class TextureGroup : IDisposable
{
    private static readonly List<TextureGroup> _allGroups = new();

    /// <summary>
    /// Gets a read-only list of all existing <see cref="TextureGroup"/> instances.
    /// </summary>
    public static IReadOnlyList<TextureGroup> AllGroups => _allGroups;

    /// <summary>
    /// Occurs when a new <see cref="TexturePage"/> is added to the group to accommodate more textures.
    /// </summary>
    public event EventHandler<TexturePage>? PageAdded;

    /// <summary>
    /// Occurs when the group has pending changes that need to be uploaded to the GPU.
    /// </summary>
    public event EventHandler? FlushRequested;

    /// <summary>
    /// Gets the name of the texture group.
    /// </summary>
    public string Name { get; } = string.Empty;

    /// <summary>
    /// Gets the size (width and height) of the texture atlases managed by this group.
    /// </summary>
    public uint AtlasSize { get; } = 0;

    /// <summary>
    /// Gets the size in pixels of each tile in the atlas, used to prevent MipMap bleeding.
    /// </summary>
    public uint TileSize { get; } = 1;

    /// <summary>
    /// Gets a value indicating whether the texture pages of this group use the sRGB color space.
    /// </summary>
    public bool IsSrgb { get; } = false;

    /// <summary>
    /// Gets or sets the factor used to calculate an inward inset for texture coordinates within the atlas.
    /// This prevents visual artifacts like color bleeding from adjacent textures.
    /// </summary>
    public float TexelInsetFactor { get; set; } = 1f / 50f; // 1/50th of a texel

    private readonly List<TexturePage> _pages = new(1);

    private readonly Dictionary<PackedTexture, TexturePage> _packedTexturesPages = new();

    private bool _flushRequested = false;

    private readonly object _packLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TextureGroup"/> class.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <param name="atlasSize">The size (width and height) of the texture atlases.</param>
    /// <param name="tileSize">The size of each tile in the atlas.</param>
    /// <param name="srgb">A value indicating whether to use the sRGB color space.</param>
    public TextureGroup(string name, uint atlasSize, uint tileSize, bool srgb = false)
    {
        this.Name = name;
        this.AtlasSize = atlasSize;
        this.TileSize = tileSize;
        this.IsSrgb = srgb;

        _allGroups.Add(this);
    }

    /// <summary>
    /// Packs a texture, defined by a draw operation, into a texture atlas.
    /// </summary>
    /// <param name="unpackedTexture">The draw operation representing the texture to pack.</param>
    /// <returns>A <see cref="PackedTexture"/> representing the location of the packed texture.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the texture is too large to fit in any page.</exception>
    public PackedTexture Pack(IDrawOperation unpackedTexture)
    {
        lock (this._packLock)
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
    }

    /// <summary>
    /// Packs a texture from a file path into a texture atlas.
    /// </summary>
    /// <param name="sourcePath">The file path of the texture to pack.</param>
    /// <returns>A <see cref="PackedTexture"/> representing the location of the packed texture.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the texture is too large to fit in any page.</exception>
    public PackedTexture Pack(string sourcePath)
    {
        return this.Pack(new TextureDrawOperation(sourcePath));
    }

    /// <summary>
    /// Requests that the texture group be flushed to update its packed texture.
    /// </summary>
    protected void RequestFlush()
    {
        if (!this._flushRequested)
        {
            this._flushRequested = true;
            this.FlushRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets a read-only list of all texture pages in the group.
    /// </summary>
    public IReadOnlyList<TexturePage> Pages => this._pages;

    /// <summary>
    /// Applies all pending packing and redraw operations to the GPU textures.
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
    /// Releases a packed texture, making its space available for new textures.
    /// </summary>
    /// <param name="packedTexture">The packed texture to release.</param>
    /// <exception cref="InvalidOperationException">Thrown if the texture is not part of this group.</exception>
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
    /// Redraws an existing packed texture region with a new texture.
    /// </summary>
    /// <param name="packedTexture">The packed texture region to update.</param>
    /// <param name="drawOperation">The draw operation for the new texture content.</param>
    /// <exception cref="InvalidOperationException">Thrown if the texture is not part of this group.</exception>
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
    /// Disposes all texture pages managed by this group.
    /// </summary>
    public void Dispose()
    {
        foreach (var page in this._pages)
        {
            page.Dispose();
        }

        _allGroups.Remove(this);
    }

    /// <summary>
    /// Saves all texture atlases in this group to PNG files.
    /// </summary>
    /// <param name="name">The base name for the output files. Each page will be saved as "{name}_{index}.png".</param>
    public void SaveToPng(string name)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(name) ?? ".");

        for (int i = 0; i < this._pages.Count; i++)
        {
            var page = this._pages[i];
            page.SaveToPng(name.Replace(".png", $"_{i}.png"));
        }
    }

    /// <summary>
    /// Saves all texture atlases of all existing texture groups to PNG files in the specified directory
    /// with filenames based on the group names.
    /// </summary>
    public static void SaveAllGroupsToPng(string directory)
    {
        foreach (var group in _allGroups)
        {
            var safeGroupName = string.IsNullOrWhiteSpace(group.Name) ? "texture_group" : group.Name.ToSnakeCase();
            group.SaveToPng(Path.Combine(directory, $"{safeGroupName}.png"));
        }
    }
}
