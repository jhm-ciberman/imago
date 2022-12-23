using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using LifeSim.Support;

namespace LifeSim.Engine.SceneGraph;

public class Node3D : IDisposable
{
    [Flags]
    private enum DirtyFlags : byte
    {
        None = 0,
        LocalMatrix = 1 << 0,
        WorldMatrix = 1 << 1,
        All = LocalMatrix | WorldMatrix
    }


    /// <summary>
    /// Gets the name of the node.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the parent of this node.
    /// </summary>
    public Node3D? Parent { get; protected set; } = null;

    private Vector3 _position = Vector3.Zero;
    private Quaternion _rotation = Quaternion.Identity;
    private Vector3 _scale = Vector3.One;

    private readonly SwapPopList<Node3D> _children = new SwapPopList<Node3D>();


    /// <summary>
    /// Gets a list of all children of this node.
    /// </summary>
    public IReadOnlyList<Node3D> Children => this._children;

    /// <summary>
    /// Gets or sets the position of the node.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the rotation of the node.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the scale of the node.
    /// </summary>
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

    /// <summary>
    /// Gets the scene this node is in or null if it is not in a scene.
    /// </summary>
    public Scene? Scene { get; protected set; } = null;

    private Matrix4x4 _localMatrix = Matrix4x4.Identity;
    private Matrix4x4 _worldMatrix = Matrix4x4.Identity;
    private DirtyFlags _dirtyFlags = DirtyFlags.All;


    /// <summary>
    /// Gets whether the local transform of this node is dirty.
    /// </summary>
    public bool LocalTransformIsDirty => (this._dirtyFlags & DirtyFlags.LocalMatrix) != 0;

    /// <summary>
    /// Gets whether the world transform of this node is dirty.
    /// </summary>
    public bool WorldTransformIsDirty => (this._dirtyFlags & DirtyFlags.WorldMatrix) != 0;

    private bool _disposedValue;

    /// <summary>
    /// Gets whether this node is disposed.
    /// </summary>
    public bool IsDisposed => this._disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="Node3D"/> class.
    /// </summary>
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

    /// <summary>
    /// Gets the local transform of this node.
    /// </summary>
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

    /// <summary>
    /// Recursively updates the world transform of this node and all its children.
    /// </summary>
    /// <param name="parentMatrix">The world transform of the parent node.</param>
    public virtual void UpdateTransform(ref Matrix4x4 parentMatrix)
    {
        this._worldMatrix = this.LocalMatrix * parentMatrix;
        this._dirtyFlags &= ~DirtyFlags.WorldMatrix;
        for (int i = 0; i < this.Children.Count; i++)
        {
            this.Children[i].UpdateTransform(ref this._worldMatrix);
        }
    }

    /// <summary>
    /// Recursively updates the world transform of this node and all its children.
    /// </summary>
    public void UpdateTransform()
    {
        Matrix4x4 mat = (this.Parent == null) ? Matrix4x4.Identity : this.Parent.WorldMatrix;
        this.UpdateTransform(ref mat);
    }

    /// <summary>
    /// Gets the world transform of this node.
    /// </summary>
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

    /// <summary>
    /// Adds a child node to this node.
    /// </summary>
    /// <param name="node">The node to add.</param>
    public void AddChild(Node3D node)
    {
        // Prevent adding self as child
        if (node.Parent == this || node == this) return;

        // If node already has a parent, remove it from that parent
        node.Parent?.RemoveChild(node);

        // Set node's parent to this
        this._children.Add(node);

        node.Parent = this;

        // If the current node has a scene, add the node to the scene
        if (this.Scene != null)
        {
            node.AttachToSceneRecursive(this.Scene);
        }
    }

    /// <summary>
    /// Removes a child node from this node.
    /// </summary>
    /// <param name="node">The node to remove.</param>
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

    /// <summary>
    /// Removes a child node from this node and disposes it.
    /// </summary>
    /// <param name="node">The node to remove and dispose.</param>
    public void RemoveAndDisposeChild(Node3D node)
    {
        this.RemoveChild(node);
        node.Dispose();
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

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                foreach (var child in this._children)
                {
                    child.Dispose();
                }
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(this.Name))
        {
            return this.GetType().Name;
        }
        else
        {
            return $"{this.GetType().Name}: {this.Name}";
        }
    }
}