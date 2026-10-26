using System;
using System.Collections.Generic;
using Imago.Graphics3D.Meshes;
using Imago.Graphics3D.SceneGraph.Nodes;
using Imago.Graphics3D.SceneGraph.Prefabs;

namespace Imago.Gltf;

/// <summary>
/// A scene read from a glTF file, ready to be instantiated. The scene owns its meshes.
/// </summary>
public sealed class GltfScene : IInstantiable, IDisposable
{
    private readonly GltfNodeInfo _root;
    private readonly IReadOnlyDictionary<int, Mesh[]> _meshes;
    private readonly IReadOnlyDictionary<int, GltfSkinInfo> _skins;

    internal GltfScene(GltfNodeInfo root, IReadOnlyDictionary<int, Mesh[]> meshes, IReadOnlyDictionary<int, GltfSkinInfo> skins)
    {
        this._root = root;
        this._meshes = meshes;
        this._skins = skins;
    }

    /// <summary>
    /// Creates a new node tree from this scene.
    /// </summary>
    /// <remarks>All the trees created from this scene share its meshes.</remarks>
    /// <returns>The root node of the new tree.</returns>
    public Node3D Instantiate()
    {
        return new GltfSceneInstantiator(this._root, this._meshes, this._skins).Instantiate();
    }

    /// <summary>
    /// Disposes the meshes of this scene.
    /// </summary>
    /// <remarks>The trees created from this scene can't be rendered after this.</remarks>
    public void Dispose()
    {
        foreach (Mesh[] meshes in this._meshes.Values)
        {
            foreach (Mesh mesh in meshes)
            {
                mesh.Dispose();
            }
        }
    }
}
