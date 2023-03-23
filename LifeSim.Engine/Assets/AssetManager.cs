using System.Collections.Generic;

namespace LifeSim.Engine.Assets;

// I know this could be more complex, but KISS for now
public class AssetManager
{
    private static AssetManager? _instance;

    /// <summary>
    /// Gets the singleton instance of the asset manager.
    /// </summary>
    public static AssetManager Instance => _instance ??= new AssetManager();

    private readonly Dictionary<string, object> _assets = new Dictionary<string, object>();

    /// <summary>
    /// Adds an asset to the asset manager.
    /// </summary>
    /// <param name="name">The name of the asset.</param>
    /// <param name="asset">The asset.</param>
    public void Add(string name, object asset)
    {
        this._assets.Add(name, asset);
    }

    /// <summary>
    /// Gets an asset from the asset manager.
    /// </summary>
    /// <typeparam name="T">The type of the asset.</typeparam>
    /// <param name="name">The name of the asset.</param>
    /// <returns>The asset.</returns>
    public T Get<T>(string name)
    {
        return (T)this._assets[name];
    }

    /// <summary>
    /// Checks if an asset exists in the asset manager.
    /// </summary>
    /// <param name="name">The name of the asset.</param>
    /// <returns>True if the asset exists, false otherwise.</returns>
    public bool Has(string name)
    {
        return this._assets.ContainsKey(name);
    }

    /// <summary>
    /// Removes an asset from the asset manager.
    /// </summary>
    /// <param name="name">The name of the asset.</param>
    public void Remove(string name)
    {
        this._assets.Remove(name);
    }
}
