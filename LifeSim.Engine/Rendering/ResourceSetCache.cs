using System;
using System.Collections.Generic;
using Veldrid;

namespace LifeSim.Engine.Rendering;

internal class ResourceSetCache : IDisposable
{
    private record struct Key(ITexture Texture, Shader Shader);

    private readonly Dictionary<Key, ResourceSet> _cachedResourceSets = new Dictionary<Key, ResourceSet>();

    private readonly ResourceFactory _factory;

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
        {
            return resourceSet;
        }

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
