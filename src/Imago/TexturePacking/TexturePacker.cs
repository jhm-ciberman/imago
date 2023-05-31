using System;
using System.Collections.Generic;

namespace Imago.TexturePacking;

public class TexturePacker
{
    /// <summary>
    /// Event that is raised when a new page is added to the packer.
    /// </summary>
    public event EventHandler<TexturePage>? PageAdded;

    /// <summary>
    /// Event raised when a flush is requested.
    /// </summary>
    public event EventHandler? FlushRequested;

    private bool _flushRequested = false;

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
    /// Gets or sets the default group.
    /// </summary>
    public TextureGroup? DefaultGroup { get; set; } = null;

    private readonly Dictionary<string, TextureGroup> _groups = new(1, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="TexturePacker"/> class.
    /// </summary>
    public TexturePacker()
    {
        //
    }

    private void OnPageAdded(object? sender, TexturePage e)
    {
        this.PageAdded?.Invoke(this, e);
    }

    private void OnFlushRequested(object? sender, EventArgs e)
    {
        if (this._flushRequested) return;

        this._flushRequested = true;
        this.FlushRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Finds the group with the specified name.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <returns>The group with the specified name.</returns>
    /// <exception cref="ArgumentException">Thrown if the group does not exist.</exception>
    public TextureGroup FindGroup(string name)
    {
        if (this._groups.TryGetValue(name, out var group))
            return group;

        throw new ArgumentException($"Group '{name}' does not exist.");
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
        if (groupName is null)
        {
            if (this.DefaultGroup is null)
                throw new InvalidOperationException("No default group is set.");

            return this.DefaultGroup.Pack(unpackedTexture);
        }

        return this.FindGroup(groupName).Pack(unpackedTexture);
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
        using var unpackedTexture = new TextureDrawOperation(path);
        return this.Pack(unpackedTexture, groupName);
    }

    /// <summary>
    /// Flushes all the changes made to the texture packer.
    /// </summary>
    public void FlushChanges()
    {
        if (!this._flushRequested) return;
        this._flushRequested = false;

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
            throw new ArgumentException($"Group '{group.Name}' already exists.");

        group.PageAdded += this.OnPageAdded;
        group.FlushRequested += this.OnFlushRequested;
        this._groups.Add(group.Name, group);

        this.DefaultGroup ??= group;
    }
}
