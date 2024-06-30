using System.Collections.Generic;
using LifeSim.Imago.Graphics.Meshes;
using LifeSim.Imago.SceneGraph.Prefabs;

namespace LifeSim.Imago.Wavefront;

public static class ObjLoader
{
    private static readonly Dictionary<string, ObjNode> _sceneCache = new Dictionary<string, ObjNode>();
    private static readonly Dictionary<(string path, string group), Mesh> _meshCache = new Dictionary<(string, string), Mesh>();

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
