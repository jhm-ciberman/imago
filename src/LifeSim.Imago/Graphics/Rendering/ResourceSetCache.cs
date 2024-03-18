using System;
using System.Collections.Generic;
using LifeSim.Imago.Graphics.Textures;
using Veldrid;
using Shader = LifeSim.Imago.Graphics.Materials.Shader;

namespace LifeSim.Imago.Graphics.Rendering;

/// <summary>
/// A cache for resource sets used in rendering. This class provides a way to reuse resource sets
/// that have already been created, which can improve performance by reducing the number of
/// resource set allocations and deallocations.
/// </summary>
internal class ResourceSetCache : IDisposable
{
    private record struct Key(ITexture Texture, Shader Shader);

    private readonly Dictionary<Key, ResourceSet> _cachedResourceSets = [];

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
    /// Gets or creates a resource set using the given texture and shader.
    /// </summary>
    /// <param name="shader">The shader to use.</param>
    /// <param name="texture">The texture to use.</param>
    /// <returns>The resource set.</returns>
    public ResourceSet GetResourceSet(Shader shader, ITexture texture)
    {
        var key = new Key(texture, shader);
        if (this._cachedResourceSets.TryGetValue(key, out var resourceSet))
            return resourceSet;

        resourceSet = this._factory.CreateResourceSet(new ResourceSetDescription(
            shader.MaterialResourceLayout, texture.VeldridTexture, texture.VeldridSampler));

        this._cachedResourceSets.Add(key, resourceSet);

        return resourceSet;
    }

    public void Dispose()
    {
        foreach (var resourceSet in this._cachedResourceSets.Values)
        {
            resourceSet.Dispose();
        }
    }
}
