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
    public IEnumerable<TexturePage> Pages
    {
        get
        {
            foreach (var group in this._groups.Values)
            {
                foreach (var page in group.Pages)
                {
                    yield return page;
                }
            }
        }
    }

    /// <summary>
    /// Gets the size of each page in the texture manager in pixels.
    /// </summary>
    public uint AtlasSize { get; }

    /// <summary>
    /// Gets the size of each tile in the texture manager in pixels.
    public uint TileSize { get; }

    /// <summary>
    /// Gets or sets the default group.
    /// </summary>
    public TextureGroup DefaultGroup { get; set; }

    private readonly Dictionary<string, TextureGroup> _groups = new(1, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="TexturePacker"/> class.
    /// </summary>
    /// <param name="atlasSize">The atlas size of the default group.</param>
    /// <param name="tileSize">The size of each tile in the texture pages of the default group.</param>
    /// <param name="srgb">Whether the texture pages of the default group should use the SRGB color space.</param>
    public TexturePacker(uint atlasSize, uint tileSize, bool srgb = false)
    {
        this.AtlasSize = atlasSize;
        this.TileSize = tileSize;
        this.DefaultGroup = new TextureGroup("Default", atlasSize, tileSize, srgb);
        this.DefaultGroup.PageAdded += this.OnPageAdded;
        this._groups.Add(this.DefaultGroup.Name, this.DefaultGroup);
    }

    private void OnPageAdded(object? sender, TexturePage e)
    {
        this.PageAdded?.Invoke(this, e);
    }

    /// <summary>
    /// Finds the group with the specified name.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <returns>The group with the specified name.</returns>
    /// <exception cref="ArgumentException">Thrown if the group does not exist.</exception>
    public TextureGroup FindGroup(string name)
    {
        if (!this._groups.TryGetValue(name, out var group))
        {
            throw new ArgumentException($"Group '{name}' does not exist.");
        }

        return group;
    }

    /// <summary>
    /// Packs the specified image and returns a packed texture.
    /// </summary>
    /// <param name="unpackedTexture">The texture to pack.</param>
    /// <param name="groupName">The name of the group to pack the texture in. If no value is specified, the default group will be used.</param>
    /// <returns>The packed texture.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the texture is already packed or it's too big to be packed in any page.</exception>
    public PackedTexture Pack(IDrawOperation unpackedTexture, string? groupName = null)
    {
        var group = groupName is null ? this.DefaultGroup : this.FindGroup(groupName);
        return group.Pack(unpackedTexture);
    }

    /// <summary>
    /// Packs the specified image from disk and returns a packed texture.
    /// </summary>
    /// <param name="path">The path to the image to pack.</param>
    /// <param name="groupName">The name of the group to pack the texture in. If no value is specified, the default group will be used.</param>
    /// <returns>The packed texture.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the texture is already packed or it's too big to be packed in any page.</exception>
    public PackedTexture Pack(string path, string? groupName = null)
    {
        return this.Pack(new TextureDrawOperation(path), groupName);
    }

    /// <summary>
    /// Flushes all the changes made to the texture packer.
    /// </summary>
    public void FlushChanges()
    {
        foreach (var group in this._groups.Values)
        {
            group.FlushChanges();
        }
    }

    /// <summary>
    /// Adds a new group to the texture packer.
    /// </summary>
    /// <param name="group">The group to add.</param>
    /// <exception cref="ArgumentException">Thrown if the group already exists.</exception>
    public void AddGroup(TextureGroup group)
    {
        if (this._groups.ContainsKey(group.Name))
        {
            throw new ArgumentException($"Group '{group.Name}' already exists.");
        }

        group.PageAdded += this.OnPageAdded;
        this._groups.Add(group.Name, group);
    }
}