using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using CommunityToolkit.Diagnostics;
using Imago.Controls;
using Imago.Support.Collections;

namespace Imago.SceneGraph.Nodes;

/// <summary>
/// Represents a node in the scene graph with hierarchy and mount lifecycle, but no transform of its own.
/// </summary>
/// <remarks>
/// A node has no position, rotation, or scale. It can still relay a parent's world transform down to its
/// children, which makes a transform-less node transparent in the transform hierarchy when it sits above
/// <see cref="Node3D"/> children. Use <see cref="Node3D"/> for anything that has a place in the world.
/// </remarks>
[ItemsMethod(nameof(AddChild))]
public class Node : IDisposable, IFormattable, IMountable
{
    /// <summary>
    /// Gets the name of the node.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the parent of this node.
    /// </summary>
    public Node? Parent { get; protected set; } = null;

    private readonly SwapPopList<Node> _children = new SwapPopList<Node>();

    /// <summary>
    /// Gets a list of all children of this node.
    /// </summary>
    public IReadOnlyList<Node> Children => this._children;

    /// <summary>
    /// Gets the 3D scene this node is mounted in, or <see langword="null"/> if not mounted.
    /// </summary>
    public Scene3D? Scene3D { get; protected set; } = null;

    /// <summary>
    /// Occurs when this node is being mounted to the root <see cref="Stage"/>.
    /// </summary>
    public event EventHandler? Mounted;

    /// <summary>
    /// Occurs when this node is being unmounted from the root <see cref="Stage"/>.
    /// </summary>
    public event EventHandler? Unmounting;

    private bool _disposedValue;

    /// <summary>
    /// Gets whether this node is disposed.
    /// </summary>
    public bool IsDisposed => this._disposedValue;

    /// <summary>
    /// Adds a child node to this node.
    /// </summary>
    /// <param name="node">The node to add.</param>
    public void AddChild(Node node)
    {
        // Already a child
        if (node.Parent == this) return;

        // Prevent adding self as child
        if (node == this) ThrowHelper.ThrowArgumentException(nameof(node), "Cannot add self as child");

        // Remove from old parent
        node.Parent?.RemoveChild(node, dispose: false);

        this._children.Add(node);

        node.Parent = this;

        if (this.Scene3D != null)
        {
            node.Mount(this.Scene3D);
        }
    }

    /// <summary>
    /// Removes a child node from this node.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    /// <param name="dispose">if set to <c>true</c> the node will be disposed.</param>
    public void RemoveChild(Node node, bool dispose = true)
    {
        if (node.Parent != this) throw new ArgumentException("Node is not a child of this node.", nameof(node));

        this._children.Remove(node);

        node.Parent = null;

        if (node.Scene3D != null)
            node.Unmount();

        if (dispose)
            node.Dispose();
    }

    /// <summary>
    /// Removes a child node from this node and disposes it.
    /// </summary>
    /// <param name="node">The node to remove and dispose.</param>
    public void RemoveAndDisposeChild(Node node)
    {
        this.RemoveChild(node);
        node.Dispose();
    }

    /// <summary>
    /// Mounts this node into the given scene, recursively mounting all children.
    /// </summary>
    /// <param name="scene">The <see cref="SceneGraph.Scene3D"/> to mount into.</param>
    /// <exception cref="InvalidOperationException">Thrown if the node is already mounted.</exception>
    public virtual void Mount(Scene3D scene)
    {
        if (this.Scene3D != null)
            throw new InvalidOperationException("Cannot mount a node that is already mounted.");

        this.Scene3D = scene;

        foreach (var child in this._children)
        {
            child.Mount(scene);
        }

        this.Mounted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Unmounts this node from the scene graph, recursively unmounting all children.
    /// </summary>
    /// <remarks>
    /// The <see cref="Scene3D"/> reference remains valid throughout this method and its overrides.
    /// It is cleared at the end of the base implementation after all children have been unmounted.
    /// </remarks>
    public virtual void Unmount()
    {
        if (this.Scene3D == null) return;

        foreach (var child in this._children)
        {
            child.Unmount();
        }

        this.Unmounting?.Invoke(this, EventArgs.Empty);
        this.Scene3D = null;
    }

    /// <summary>
    /// Gets the world transform of this node.
    /// </summary>
    public virtual Matrix4x4 WorldMatrix => this.Parent?.WorldMatrix ?? Matrix4x4.Identity;

    /// <summary>
    /// Relays the parent's world transform to all children.
    /// </summary>
    /// <param name="parentMatrix">The world transform of the parent node.</param>
    /// <remarks>
    /// A node has no transform of its own, so it forwards the parent transform to its children unchanged.
    /// <see cref="Node3D"/> overrides this to apply its own local transform before relaying the result.
    /// </remarks>
    public virtual void UpdateTransform(ref Matrix4x4 parentMatrix)
    {
        for (int i = 0; i < this._children.Count; i++)
        {
            this._children[i].UpdateTransform(ref parentMatrix);
        }
    }

    /// <summary>
    /// Marks the world transform of every descendant node as dirty.
    /// </summary>
    /// <remarks>
    /// A node has no transform of its own, so it just forwards the signal to its children.
    /// <see cref="Node3D"/> overrides this to also mark its own world transform dirty.
    /// </remarks>
    internal virtual void PropagateWorldTransformDirty()
    {
        for (int i = 0; i < this._children.Count; i++)
        {
            this._children[i].PropagateWorldTransformDirty();
        }
    }

    /// <summary>
    /// Returns a string representation of this node.
    /// </summary>
    /// <param name="format">The format string. Each character selects a field to append.</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <returns>A string representation of this node.</returns>
    /// <remarks>
    /// Available formats:
    /// "G": General (same as "N")
    /// "N": Name
    /// "P": Position
    /// "R": Rotation
    /// "S": Scale
    /// Can be combined, e.g. "NPRS" for name, position, rotation and scale.
    /// "P", "R", and "S" apply to a <see cref="Node3D"/>; a plain <see cref="Node"/> has no transform, so they emit nothing.
    /// </remarks>
    public string ToString(string? format = null, IFormatProvider? formatProvider = null)
    {
        if (string.IsNullOrEmpty(format))
            return this.Name;

        var sb = new StringBuilder();

        foreach (var c in format)
        {
            this.AppendFormat(sb, c);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Appends the field selected by a single format character to the builder.
    /// </summary>
    /// <param name="sb">The builder to append to.</param>
    /// <param name="format">The format character.</param>
    protected virtual void AppendFormat(StringBuilder sb, char format)
    {
        switch (format)
        {
            case 'G': // General
            case 'N':
                sb.Append(this.Name);
                break;
            case 'P': // Position
            case 'R': // Rotation
            case 'S': // Scale
                // Transform fields: a plain node has none, so emit nothing. Node3D fills these in.
                break;
            default:
                sb.Append(format); // Unknown format character, just append it
                break;
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
}
