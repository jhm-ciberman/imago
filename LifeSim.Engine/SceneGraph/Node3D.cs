using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph;

public class Node3D
{
    [Flags]
    private enum DirtyFlags
    {
        None = 0,
        LocalMatrix = 1 << 0,
        WorldMatrix = 1 << 1,
        All = LocalMatrix | WorldMatrix
    }


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
            this.NotifyLocalTransformDirty();
        }
    }

    public Quaternion Rotation
    {
        get => this._rotation;
        set
        {
            if (this._rotation == value) return;
            this._rotation = value;
            this.NotifyLocalTransformDirty();
        }
    }

    public Vector3 Scale
    {
        get => this._scale;
        set
        {
            if (this._scale == value) return;
            this._scale = value;
            this.NotifyLocalTransformDirty();
        }
    }

    public Scene? Scene { get; protected set; } = null;

    private Matrix4x4 _localMatrix = Matrix4x4.Identity;
    private Matrix4x4 _worldMatrix = Matrix4x4.Identity;
    private DirtyFlags _dirtyFlags = DirtyFlags.All;

    public bool LocalTransformIsDirty => (this._dirtyFlags & DirtyFlags.LocalMatrix) != 0;
    public bool WorldTransformIsDirty => (this._dirtyFlags & DirtyFlags.WorldMatrix) != 0;

    public Node3D()
    {
        //
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void NotifyLocalTransformDirty()
    {
        if (this.LocalTransformIsDirty) return;
        this._dirtyFlags |= DirtyFlags.LocalMatrix | DirtyFlags.WorldMatrix;
        this.Scene?.NotifyTransformDirty(this);

        foreach (var child in this._children)
        {
            child.NotifyWorldTransformDirty();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void NotifyWorldTransformDirty()
    {
        if (this.WorldTransformIsDirty) return;
        this._dirtyFlags |= DirtyFlags.WorldMatrix;
        this.Scene?.NotifyTransformDirty(this);

        foreach (var child in this._children)
        {
            child.NotifyWorldTransformDirty();
        }
    }

    public ref Matrix4x4 LocalMatrix
    {
        get
        {
            if (this.LocalTransformIsDirty)
            {
                this._localMatrix = Matrix4x4.CreateScale(this._scale)
                    * Matrix4x4.CreateFromQuaternion(this._rotation)
                    * Matrix4x4.CreateTranslation(this._position);
                this._dirtyFlags &= ~DirtyFlags.LocalMatrix;
            }
            return ref this._localMatrix;
        }
    }

    public virtual void UpdateTransform(ref Matrix4x4 parentMatrix)
    {
        this._worldMatrix = this.LocalMatrix * parentMatrix;
        this._dirtyFlags &= ~DirtyFlags.WorldMatrix;
        for (int i = 0; i < this.Children.Count; i++)
        {
            this.Children[i].UpdateTransform(ref this._worldMatrix);
        }
    }

    public void UpdateTransform()
    {
        Matrix4x4 mat = (this.Parent == null) ? Matrix4x4.Identity : this.Parent.WorldMatrix;
        this.UpdateTransform(ref mat);
    }

    public ref Matrix4x4 WorldMatrix
    {
        get
        {
            if (this.WorldTransformIsDirty)
            {
                this.UpdateTransform();
            }
            return ref this._worldMatrix;
        }
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

    public T FindChildOrFail<T>(string name) where T : Node3D
    {
        var found = this.FindChild<T>(name);
        if (found == null) throw new InvalidOperationException($"Child with name '{name}' not found.");
        return found;
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

    public T FindPathOrFail<T>(string path) where T : Node3D
    {
        var found = this.FindPath<T>(path);
        if (found == null) throw new InvalidOperationException($"Path '{path}' not found.");
        return found;
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
        this.Scene.NotifyTransformDirty(this);

        foreach (var child in this._children)
        {
            child.AttachToSceneRecursive(scene);
        }
    }

    internal virtual void DetachFromSceneRecursive()
    {
        if (this.Scene == null) return;

        this.Scene.NotifyTransformNotDirty(this);
        this.Scene = null;

        foreach (var child in this._children)
        {
            child.DetachFromSceneRecursive();
        }
    }

    public virtual void PrintHierarchyToConsole(string indent = "")
    {
        Console.WriteLine($"{indent}{this.Name} (Scale: {this.Scale})");
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


}