using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph;

public class Node3D
{

    public string Name { get; set; } = string.Empty;

    public Node3D? Parent { get; protected set; } = null;

    private Vector3    _position = Vector3.Zero;
    private Quaternion _rotation = Quaternion.Identity;
    private Vector3    _scale = Vector3.One;

    private readonly SwapPopList<Node3D> _children = new SwapPopList<Node3D>();


    public IReadOnlyList<Node3D> Children => this._children;


    public Vector3 Position
    {
        get => this._position;
        set
        {
            if (this._position == value) return;
            this._position = value;
            this.NotifyTransformDirty();
        }
    }

    public Quaternion Rotation
    {
        get => this._rotation;
        set
        {
            if (this._rotation == value) return;
            this._rotation = value;
            this.NotifyTransformDirty();
        }
    }

    public Vector3 Scale
    {
        get => this._scale;
        set
        {
            if (this._scale == value) return;
            this._scale = value;
            this.NotifyTransformDirty();
        }
    }

    public Scene? Scene { get; protected set; } = null;

    private Matrix4x4 _localMatrix = Matrix4x4.Identity;

    protected Matrix4x4 _worldMatrix = Matrix4x4.Identity;
    public ref Matrix4x4 WorldMatrix => ref this._worldMatrix;

    protected bool _transformIsDirty = true;
    public bool TransformIsDirty => this._transformIsDirty;

    public Vector3 WorldPosition => Vector3.Transform(Vector3.Zero, this._worldMatrix);
    public Vector3 WorldScale => Vector3.Transform(this._scale, this._worldMatrix);

    public Node3D()
    {
        //
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void NotifyTransformDirty()
    {
        if (this._transformIsDirty) return;
        this._transformIsDirty = true;
        this.Scene?.NotifyTransformDirty(this);
    }

    public ref Matrix4x4 GetLocalMatrix()
    {
        if (this._transformIsDirty)
        {
            this._localMatrix = Matrix4x4.CreateScale(this._scale)
                * Matrix4x4.CreateFromQuaternion(this._rotation)
                * Matrix4x4.CreateTranslation(this._position);
            this._transformIsDirty = false;
        }
        return ref this._localMatrix;
    }

    public T? FindChild<T>(string name) where T : Node3D
    {
        if (this is T tNode && this.Name == name) return tNode;

        foreach (var child in this.Children)
        {
            var found = child.FindChild<T>(name);
            if (found != null) return found;
        }

        return null;
    }

    public T? GetChildByName<T>(string name) where T : Node3D
    {
        foreach (var child in this.Children)
        {
            if (child.Name == name && child is T tChild)
            {
                return tChild;
            }
        }

        return null;
    }

    public Node3D? GetChildByName(string name)
    {
        foreach (var child in this.Children)
        {
            if (child.Name == name) return child;
        }

        return null;
    }

    // path is a relative path to a node (example: "Armature/Hips/Spine1/Spine2/Head")
    public T? FindPath<T>(string path) where T : Node3D
    {
        if (string.IsNullOrEmpty(path)) return null;

        var pathParts = path.Split('/');
        var currentNode = this;
        foreach (var pathPart in pathParts)
        {
            currentNode = currentNode.GetChildByName(pathPart);
            if (currentNode == null) return null;
        }

        return currentNode as T;
    }

    public void AddChild(Node3D node)
    {
        // Prevent adding self as child
        if (node.Parent == this || node == this) return;

        // If node already has a parent, remove it from that parent
        if (node.Parent != null)
        {
            node.Parent.RemoveChild(node);
        }

        // Set node's parent to this
        this._children.Add(node);

        node.Parent = this;

        // If the current node has a scene, add the node to the scene
        if (this.Scene != null)
        {
            node.AttachToSceneRecursive(this.Scene);
        }
    }

    public void RemoveChild(Node3D node)
    {
        if (node.Parent != this) return;

        this._children.Remove(node);

        node.Parent = null;

        if (node.Scene != null)
        {
            node.DetachFromSceneRecursive();
        }
    }

    internal virtual void AttachToSceneRecursive(Scene scene)
    {
        if (this.Scene != null) return;

        this.Scene = scene;
        this.Scene.NotifyNodeAdded(this);

        foreach (var child in this._children)
        {
            child.AttachToSceneRecursive(scene);
        }
    }

    internal virtual void DetachFromSceneRecursive()
    {
        if (this.Scene == null) return;

        this.Scene.NotifyNodeRemoved(this);
        this.Scene = null;

        foreach (var child in this._children)
        {
            child.DetachFromSceneRecursive();
        }
    }

    public virtual void PrintHierarchyToConsole(string indent = "")
    {
        Console.WriteLine($"{indent}{this.Name}");
        foreach (var child in this._children)
        {
            child.PrintHierarchyToConsole($"{indent}  ");
        }
    }

    public void ForEachRecursive<T>(Action<T> action)
    {
        if (this is T node)
        {
            action(node);
        }
        foreach (var child in this._children)
        {
            child.ForEachRecursive<T>(action);
        }
    }

    public virtual void UpdateWorldMatrix(ref Matrix4x4 parentMatrix)
    {
        this._worldMatrix = this.GetLocalMatrix() * parentMatrix;

        for (int i = 0; i < this.Children.Count; i++)
        {
            this.Children[i].UpdateWorldMatrix(ref this._worldMatrix);
        }
    }
}