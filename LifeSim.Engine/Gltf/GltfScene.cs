using System.Collections.Generic;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Gltf;

internal class GltfScene : IScenePrefab
{
    public string Name { get; }
    private readonly List<GLTFNode> _children = new List<GLTFNode>();
    private readonly Dictionary<string, GLTFNode> _nodesByName = new Dictionary<string, GLTFNode>();

    public GltfScene(string name)
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

    public Node3D Instantiate()
    {
        return new SceneInstantiator().Instantiate(this);
    }

    public void Dispose()
    {
        // TODO: Dispose of all children.
    }
}
