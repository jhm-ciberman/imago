using System;
using System.Collections.Generic;
using Imago.Assets.Textures;
using NeoVeldrid;

namespace Imago.Rendering.Internals;

/// <summary>
/// A cache for resource sets used in rendering. This class provides a way to reuse resource sets
/// that have already been created
/// </summary>
public class ResourceSetCache : IDisposable
{
    private record struct Key(ITexture Texture, ResourceLayout Layout);

    private readonly Dictionary<Key, ResourceSet> _cachedResourceSets = new Dictionary<Key, ResourceSet>();

    private readonly ResourceFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceSetCache"/> class.
    /// </summary>
    /// <param name="factory">The resource factory.</param>
    public ResourceSetCache(ResourceFactory factory)
    {
        this._factory = factory;
    }

    /// <summary>
    /// Gets or creates a resource set using the given layout and texture.
    /// </summary>
    /// <param name="layout">The resource layout to use. It must hold one texture and one sampler.</param>
    /// <param name="texture">The texture to use.</param>
    /// <returns>The resource set.</returns>
    public ResourceSet GetResourceSet(ResourceLayout layout, ITexture texture)
    {
        var key = new Key(texture, layout);
        if (this._cachedResourceSets.TryGetValue(key, out var resourceSet))
            return resourceSet;

        resourceSet = this._factory.CreateResourceSet(new ResourceSetDescription(
            layout, texture.NativeTexture, texture.NativeSampler));

        this._cachedResourceSets.Add(key, resourceSet);

        return resourceSet;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var resourceSet in this._cachedResourceSets.Values)
        {
            resourceSet.Dispose();
        }
    }
}
