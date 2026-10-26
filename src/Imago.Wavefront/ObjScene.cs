using System;
using System.Collections.Generic;
using Imago.Graphics3D.Meshes;
using Imago.Graphics3D.SceneGraph.Nodes;
using Imago.Graphics3D.SceneGraph.Prefabs;

namespace Imago.Wavefront;

/// <summary>
/// A scene read from an OBJ file, ready to be instantiated. The scene owns its meshes.
/// </summary>
public sealed class ObjScene : IInstantiable, IDisposable
{
    private readonly string? _name;
    private readonly IReadOnlyList<KeyValuePair<string, Mesh>> _groups;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjScene"/> class for a single group.
    /// </summary>
    /// <param name="mesh">The mesh of the group.</param>
    internal ObjScene(Mesh mesh)
    {
        this._groups = [new KeyValuePair<string, Mesh>(string.Empty, mesh)];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjScene"/> class for many groups under one root node.
    /// </summary>
    /// <param name="name">The name of the root node.</param>
    /// <param name="groups">The mesh of each group, by group name.</param>
    internal ObjScene(string name, IReadOnlyList<KeyValuePair<string, Mesh>> groups)
    {
        this._name = name;
        this._groups = groups;
    }

    /// <summary>
    /// Creates a new node tree from this scene.
    /// </summary>
    /// <remarks>All the trees created from this scene share its meshes.</remarks>
    /// <returns>The root node of the new tree.</returns>
    public Node3D Instantiate()
    {
        if (this._name == null)
        {
            return new RenderNode3D(this._groups[0].Value);
        }

        var node = new Node3D { Name = this._name };
        foreach (var group in this._groups)
        {
            node.AddChild(new RenderNode3D { Name = group.Key, Mesh = group.Value });
        }

        return node;
    }

    /// <summary>
    /// Disposes the meshes of this scene.
    /// </summary>
    /// <remarks>The trees created from this scene can't be rendered after this.</remarks>
    public void Dispose()
    {
        foreach (var group in this._groups)
        {
            group.Value.Dispose();
        }
    }
}
