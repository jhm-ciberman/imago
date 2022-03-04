using System;
using System.Collections.Generic;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class ResourceSetCache : IDisposable
{
    private struct CachedResourceSetKey
    {
        public ITexture Texture { get; set; }
        public Shader Shader { get; set; }

        public CachedResourceSetKey(ITexture texture, Shader shader)
        {
            this.Texture = texture;
            this.Shader = shader;
        }
    }

    private readonly Dictionary<CachedResourceSetKey, ResourceSet> _cachedResourceSets = new Dictionary<CachedResourceSetKey, ResourceSet>();

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
        var key = new CachedResourceSetKey(texture, shader);
        if (this._cachedResourceSets.TryGetValue(key, out var resourceSet))
        {
            return resourceSet;
        }

        resourceSet = this._factory.CreateResourceSet(new ResourceSetDescription(
            shader.MaterialResourceLayout, texture.DeviceTexture, texture.Sampler));

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