using System.Collections.Generic;
using LifeSim.Imago.Assets.Meshes;
using LifeSim.Imago.SceneGraph.Prefabs;

namespace LifeSim.Imago.Assets.Wavefront;

/// <summary>
/// Provides static methods for loading Wavefront OBJ files.
/// </summary>
public static class ObjLoader
{
    private static readonly Dictionary<string, ObjNode> _sceneCache = new Dictionary<string, ObjNode>();
    private static readonly Dictionary<(string path, string group), Mesh> _meshCache = new Dictionary<(string, string), Mesh>();

    /// <summary>
    /// Loads an OBJ file as a scene prefab.
    /// </summary>
    /// <param name="path">The file path of the OBJ asset.</param>
    /// <param name="rootNode">The name of the group to use as the root node. If null, the entire scene is loaded.</param>
    /// <returns>An <see cref="IInstantiable"/> representing the loaded scene or mesh prefab.</returns>
    public static IInstantiable LoadScenePrefab(string path, string? rootNode = null)
    {
        if (!_sceneCache.TryGetValue(path, out ObjNode? scene))
        {
            var parser = new ObjParser();
            scene = parser.LoadScene(path);
            _sceneCache.Add(path, scene);
        }

        if (rootNode != null)
        {
            var mesh = scene.FindGroup(rootNode) ?? throw new KeyNotFoundException($"Root node {rootNode} not found in scene {path}");

            return new MeshPrefab(mesh);
        }

        return scene;
    }

    /// <summary>
    /// Loads a single mesh from an OBJ file by its group name.
    /// </summary>
    /// <param name="path">The file path of the OBJ asset.</param>
    /// <param name="groupName">The name of the group to load as a mesh.</param>
    /// <returns>The loaded <see cref="Mesh"/>, or null if the group was not found.</returns>
    public static Mesh? LoadMeshByGroupName(string path, string groupName)
    {
        var key = (path, groupName);
        if (!_meshCache.TryGetValue(key, out Mesh? mesh))
        {
            var parser = new ObjParser();
            mesh = parser.LoadMeshByGroupName(path, groupName);
            if (mesh != null)
            {
                _meshCache.Add(key, mesh);
            }
        }

        return mesh;
    }
}
