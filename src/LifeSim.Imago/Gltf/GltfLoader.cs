using System;
using System.Collections.Generic;
using System.Linq;
using LifeSim.Imago.Animations;
using LifeSim.Imago.Graphics.Meshes;

namespace LifeSim.Imago.Gltf;

public class GltfLoader
{
    private static readonly Dictionary<string, GltfAsset> _cache = new Dictionary<string, GltfAsset>();

    public static GltfAsset LoadFile(string path)
    {
        if (!_cache.TryGetValue(path, out GltfAsset? asset))
        {
            var reader = new GltfReader(path);
            asset = reader.Load();
            _cache.Add(path, asset);
        }

        return asset;
    }

    public static Animation LoadAnimation(string path, string? animationName = null)
    {
        var gltf = LoadFile(path);
        return string.IsNullOrEmpty(animationName)
            ? gltf.Animations[0]
            : gltf.Animations.FirstOrDefault(a => a.Name == animationName) ?? throw new Exception($"Could not find animation with name {animationName}");
    }

    public static GltfNode LoadScenePrefab(string path, string? sceneName = null, string? rootNodeName = null)
    {
        var gltf = LoadFile(path);
        var scene = string.IsNullOrEmpty(sceneName)
            ? gltf.Scene
            : gltf.Scenes.First(s => s.Name == sceneName);

        return string.IsNullOrEmpty(rootNodeName)
            ? scene
            : scene.FindNodeByName(rootNodeName) ?? throw new Exception($"Could not find node with name {rootNodeName}");
    }

    public static Mesh[] LoadMeshes(string path, string? sceneName = null, string? rootNodeName = null)
    {
        var gltf = LoadFile(path);
        var scene = string.IsNullOrEmpty(sceneName)
            ? gltf.Scene
            : gltf.Scenes.First(s => s.Name == sceneName);

        var node = string.IsNullOrEmpty(rootNodeName)
            ? scene
            : scene.FindNodeByName(rootNodeName) ?? throw new Exception($"Could not find node with name {rootNodeName}");

        return node.Meshes;
    }

    public static Mesh LoadMesh(string path, string? sceneName = null, string? rootNodeName = null, int index = 0)
    {
        return LoadMeshes(path, sceneName, rootNodeName)[index];
    }
}
