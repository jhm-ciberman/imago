using System;
using System.Collections.Generic;
using System.IO;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.Resources;

namespace LifeSim.Engine;

public static class AssetManager
{
    public delegate object LoadAssetDelegate(string path);

    private static readonly Dictionary<string, object> _loadedAssets = new Dictionary<string, object>();

    private static readonly Dictionary<Type, LoadAssetDelegate> _loaders = new Dictionary<Type, LoadAssetDelegate>();

    public static string ResourcesPath { get; set; } = "./res/";

    static AssetManager()
    {
        _loaders.Add(typeof(Texture), LoadTexture);
    }

    private static object LoadTexture(string path)
    {
        return new ImageTexture(path);
    }

    private static string PathResolver(string path)
    {
        return Path.Combine(ResourcesPath, path);
    }

    public static void RegisterLoader<T>(LoadAssetDelegate loader)
    {
        _loaders.Add(typeof(T), loader);
    }

    public static T Load<T>(string path)
    {
        if (_loadedAssets.ContainsKey(path))
        {
            return (T)_loadedAssets[path];
        }

        var loader = _loaders[typeof(T)];
        var fullPath = PathResolver(path);
        var asset = loader(fullPath);
        _loadedAssets.Add(path, asset);
        return (T)asset;
    }

    public static void Unload(string path)
    {
        if (_loadedAssets.TryGetValue(path, out object? asset))
        {
            if (asset is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _loadedAssets.Remove(path);
        }
    }

    public static void UnloadAll()
    {
        foreach (var asset in _loadedAssets)
        {
            if (asset.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        _loadedAssets.Clear();
    }
}