using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using CommunityToolkit.Diagnostics;
using Imago.Support.Collections;

namespace Imago.SceneGraph.Nodes;

/// <summary>
/// Represents a 3D node in the scene graph with transformation, hierarchy, and lifecycle management.
/// </summary>
public class Node3D : IDisposable, IFormattable
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
    /// Gets the 3D layer this node is attached to, or null if not attached.
    /// </summary>
    public Layer3D? Layer3D { get; protected set; } = null;

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
        this.Layer3D?.NotifyTransformDirty(this);

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
        this.Layer3D?.NotifyTransformDirty(this);

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
        this._dirtyFlags &= ~DirtyFlags.WorldMatrix;

        if (this.IsDisposed) return;

        this._worldMatrix = this.LocalMatrix * parentMatrix;
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
        Matrix4x4 mat = this.Parent == null ? Matrix4x4.Identity : this.Parent.WorldMatrix;
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
                this.UpdateTransform();
            return ref this._worldMatrix;
        }
    }

    /// <summary>
    /// Gets the world position of this node.
    /// </summary>
    public Vector3 WorldPosition
    {
        get
        {
            var mat = this.WorldMatrix;
            return new Vector3(mat.M41, mat.M42, mat.M43);
        }
    }

    /// <summary>
    /// Gets the world rotation of this node as a quaternion.
    /// </summary>
    public Quaternion WorldRotation
    {
        get
        {
            var mat = this.WorldMatrix;
            return Quaternion.CreateFromRotationMatrix(new Matrix4x4(
                mat.M11, mat.M12, mat.M13, 0f,
                mat.M21, mat.M22, mat.M23, 0f,
                mat.M31, mat.M32, mat.M33, 0f,
                0f, 0f, 0f, 1f));
        }
    }

    /// <summary>
    /// Adds a child node to this node.
    /// </summary>
    /// <param name="node">The node to add.</param>
    public void AddChild(Node3D node)
    {
        // Already a child
        if (node.Parent == this) return;

        // Prevent adding self as child
        if (node == this) ThrowHelper.ThrowArgumentException(nameof(node), "Cannot add self as child");

        // Remove from old parent
        node.Parent?.RemoveChild(node, dispose: false);

        // Set node's parent to this
        this._children.Add(node);

        node.Parent = this;

        // If the current node has a layer, add the node to the layer
        if (this.Layer3D != null)
        {
            node.AttachToLayer(this.Layer3D);
        }
    }

    /// <summary>
    /// Removes a child node from this node.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    /// <param name="dispose">if set to <c>true</c> the node will be disposed.</param>
    public void RemoveChild(Node3D node, bool dispose = true)
    {
        if (node.Parent != this) throw new ArgumentException("Node is not a child of this node.", nameof(node));

        this._children.Remove(node);

        node.Parent = null;

        if (node.Layer3D != null)
            node.DetachFromLayer();

        if (dispose)
            node.Dispose();
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

    /// <summary>
    /// Attaches this node to the given 3D layer.
    /// </summary>
    /// <param name="layer">The layer to attach to.</param>
    public virtual void AttachToLayer(Layer3D layer)
    {
        if (this.Layer3D != null)
            throw new InvalidOperationException("Cannot attach to layer if already attached to one. Please detach first.");

        this.Layer3D = layer;
        this.Layer3D.NotifyTransformDirty(this);

        foreach (var child in this._children)
        {
            child.AttachToLayer(layer);
        }
    }

    /// <summary>
    /// Detaches this node from the current 3D layer.
    /// </summary>
    public virtual void DetachFromLayer()
    {
        if (this.Layer3D == null) return;

        this.Layer3D.NotifyTransformNotDirty(this);
        this.Layer3D = null;

        foreach (var child in this._children)
        {
            child.DetachFromLayer();
        }
    }

    /// <summary>
    /// Disposes the node and releases associated resources.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources; otherwise, false.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this._disposedValue)
        {
            if (disposing)
            {
                foreach (var child in this._children)
                {
                    child.Dispose();
                }
            }

            this._disposedValue = true;
        }
    }

    /// <summary>
    /// Disposes the node and releases associated resources.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Returns a string representation of this node.
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <returns>A string representation of this node.</returns>
    /// <remarks>
    /// Available formats:
    /// "G": General (same as "N")
    /// "N": Name
    /// "P": Position
    /// "R": Rotation
    /// "S": Scale
    /// Can be combined, e.g. "NPRS" for name, position, rotation and scale
    /// </remarks>
    public string ToString(string? format = null, IFormatProvider? formatProvider = null)
    {
        if (string.IsNullOrEmpty(format))
            return this.Name;

        var sb = new System.Text.StringBuilder();

        foreach (var c in format)
        {
            switch (c)
            {
                case 'G': // General
                case 'N':
                    sb.Append(this.Name);
                    break;
                case 'P':
                    sb.Append(this.Position);
                    break;
                case 'R':
                    sb.Append(this.Rotation);
                    break;
                case 'S':
                    sb.Append(this.Scale);
                    break;
                default:
                    sb.Append(c); // Unknown format character, just append it
                    break;
            }
        }

        return sb.ToString();
    }
}
