using System.Collections.Generic;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;
using LifeSim.Engine;

namespace LifeSim.Engine.Gltf;

internal class GLTFScene : IScenePrefab
{
    private readonly List<GLTFNode> _children = new List<GLTFNode>();
    private readonly Dictionary<string, GLTFNode> _nodesByName = new Dictionary<string, GLTFNode>();
    public readonly string Name;

    public GLTFScene(string name)
    {
        this.Name = name;
    }

    public IReadOnlyList<GLTFNode> Children => this._children;

    public GLTFNode? FindNodeByName(string name)
    {
        return this._nodesByName.GetValueOrDefault(name);
    }

    public void Add(GLTFNode node)
    {
        this._children.Add(node);
        this.AddToDictionaryRecursive(node);
    }

    private void AddToDictionaryRecursive(GLTFNode node)
    {
        this._nodesByName[node.Name] = node;
        foreach (GLTFNode? child in node.Children)
        {
            this.AddToDictionaryRecursive(child);
        }
    }

    public Node3D Instantiate(SceneStorage storage)
    {
        return new SceneInstantiator(storage).Instantiate(this);
    }
}