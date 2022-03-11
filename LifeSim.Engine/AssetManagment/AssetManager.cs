using System;
using System.Collections.Generic;

namespace LifeSim.Engine.AssetManagment;

// The AssetManager has an internal cache <path, object> to avoid loading the same asset twice.
public class AssetManager
{
    private static AssetManager? _default = null;

    public static AssetManager Default
    {
        get
        {
            if (_default == null)
            {
                _default = new AssetManager();
            }

            return _default;
        }
    }

    // (filepath, asset definition)
    private readonly Dictionary<string, IAsset> _definitions = new Dictionary<string, IAsset>();

    // (key, asset)
    private readonly Dictionary<string, object> _loadedAssets = new Dictionary<string, object>();

    public void RegisterAsset(IAsset asset)
    {
        if (this._definitions.ContainsKey(asset.Key))
        {
            throw new ArgumentException($"Asset with key {asset.Key} already registered.");
        }

        this._definitions.Add(asset.Key, asset);
    }

    public void RegisterAssets(IAsset[] definitions)
    {
        foreach (var definition in definitions)
        {
            this.RegisterAsset(definition);
        }
    }

    public T LoadAsset<T>(string key) where T : class
    {
        if (this._loadedAssets.TryGetValue(key, out object? asset))
        {
            return ReturnTypedAsset<T>(asset);
        }

        if (!this._definitions.TryGetValue(key, out IAsset? definition))
        {
            throw new ArgumentException($"Asset with key {key} not registered.");
        }

        asset = definition.Load();

        this._loadedAssets.Add(key, asset);

        return ReturnTypedAsset<T>(asset);
    }

    public T Get<T>(string key) where T : class
    {
        if (!this._loadedAssets.TryGetValue(key, out object? asset))
        {
            asset = this.LoadAsset<T>(key);
        }

        return ReturnTypedAsset<T>(asset);
    }

    public T GetOrLoad<T>(string key) where T : class
    {
        if (!this._loadedAssets.TryGetValue(key, out object? asset))
        {
            throw new ArgumentException($"Asset with key {key} not loaded.");
        }

        return ReturnTypedAsset<T>(asset);
    }

    private static T ReturnTypedAsset<T>(object obj)
    {
        if (obj is T typedObj)
        {
            return typedObj;
        }

        throw new ArgumentException($"Asset is not of type {typeof(T).Name}. It is of type {obj.GetType().Name}.");
    }

    public void UnloadAsset(string key)
    {
        if (!this._loadedAssets.TryGetValue(key, out object? asset))
        {
            throw new ArgumentException($"Asset with key {key} not loaded.");
        }

        if (asset is IDisposable disposable)
        {
            disposable.Dispose();
        }

        this._loadedAssets.Remove(key);
    }

    public void UnloadAllAssets()
    {
        foreach (var asset in this._loadedAssets.Values)
        {
            if (asset is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        this._loadedAssets.Clear();
    }

    public void PreloadAllAssets()
    {
        foreach (var asset in this._definitions.Values)
        {
            if (this._loadedAssets.ContainsKey(asset.Key))
            {
                continue;
            }

            this._loadedAssets.Add(asset.Key, asset.Load());
        }
    }
}