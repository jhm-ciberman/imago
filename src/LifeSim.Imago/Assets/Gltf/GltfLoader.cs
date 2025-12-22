using System;
using System.Collections.Generic;
using System.Linq;
using LifeSim.Imago.Assets.Animations;
using LifeSim.Imago.Assets.Meshes;

namespace LifeSim.Imago.Assets.Gltf;

/// <summary>
/// Provides static methods for loading glTF assets and their components.
/// </summary>
public class GltfLoader
{
    private static readonly Dictionary<string, GltfAsset> _cache = new Dictionary<string, GltfAsset>();

    /// <summary>
    /// Loads a glTF file and caches it for subsequent requests.
    /// </summary>
    /// <param name="path">The file path of the glTF asset.</param>
    /// <returns>The loaded <see cref="GltfAsset"/>.</returns>
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

    /// <summary>
    /// Loads a specific animation from a glTF file.
    /// </summary>
    /// <param name="path">The file path of the glTF asset.</param>
    /// <param name="animationName">The name of the animation to load. If null, the first animation is loaded.</param>
    /// <returns>The loaded <see cref="Animation"/>.</returns>
    public static Animation LoadAnimation(string path, string? animationName = null)
    {
        var gltf = LoadFile(path);
        return string.IsNullOrEmpty(animationName)
            ? gltf.Animations[0]
            : gltf.Animations.FirstOrDefault(a => a.Name == animationName) ?? throw new Exception($"Could not find animation with name {animationName}");
    }

    /// <summary>
    /// Loads a scene or a specific node from a glTF file to be used as a prefab.
    /// </summary>
    /// <param name="path">The file path of the glTF asset.</param>
    /// <param name="sceneName">The name of the scene to load. If null, the default scene is used.</param>
    /// <param name="rootNodeName">The name of the node to use as the root. If null, the scene's root is used.</param>
    /// <returns>The loaded <see cref="GltfNode"/> as a prefab.</returns>
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

    /// <summary>
    /// Loads all meshes from a specific node in a glTF file.
    /// </summary>
    /// <param name="path">The file path of the glTF asset.</param>
    /// <param name="sceneName">The name of the scene to load from. If null, the default scene is used.</param>
    /// <param name="rootNodeName">The name of the node to load meshes from. If null, the scene's root is used.</param>
    /// <returns>An array of loaded <see cref="Mesh"/> objects.</returns>
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

    /// <summary>
    /// Loads a single mesh from a specific node in a glTF file.
    /// </summary>
    /// <param name="path">The file path of the glTF asset.</param>
    /// <param name="sceneName">The name of the scene to load from. If null, the default scene is used.</param>
    /// <param name="rootNodeName">The name of the node to load the mesh from. If null, the scene's root is used.</param>
    /// <param name="index">The index of the mesh to load from the node's mesh array.</param>
    /// <returns>The loaded <see cref="Mesh"/>.</returns>
    public static Mesh LoadMesh(string path, string? sceneName = null, string? rootNodeName = null, int index = 0)
    {
        return LoadMeshes(path, sceneName, rootNodeName)[index];
    }
}
