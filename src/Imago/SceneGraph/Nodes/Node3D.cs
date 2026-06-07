using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Imago.SceneGraph.Nodes;

/// <summary>
/// Represents a <see cref="Node"/> with a local transform and a position in the scene's transform hierarchy.
/// </summary>
public class Node3D : Node
{
    [Flags]
    private enum DirtyFlags : byte
    {
        None = 0,
        LocalMatrix = 1 << 0,
        WorldMatrix = 1 << 1,
        All = LocalMatrix | WorldMatrix
    }

    private Vector3 _position = Vector3.Zero;
    private Quaternion _rotation = Quaternion.Identity;
    private Vector3 _scale = Vector3.One;

    private Matrix4x4 _localMatrix = Matrix4x4.Identity;
    private Matrix4x4 _worldMatrix = Matrix4x4.Identity;
    private DirtyFlags _dirtyFlags = DirtyFlags.All;

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
    /// Gets whether the local transform of this node is dirty.
    /// </summary>
    public bool LocalTransformIsDirty => (this._dirtyFlags & DirtyFlags.LocalMatrix) != 0;

    /// <summary>
    /// Gets whether the world transform of this node is dirty.
    /// </summary>
    public bool WorldTransformIsDirty => (this._dirtyFlags & DirtyFlags.WorldMatrix) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void NotifyLocalTransformDirty()
    {
        if (this.LocalTransformIsDirty) return;
        this._dirtyFlags |= DirtyFlags.LocalMatrix | DirtyFlags.WorldMatrix;
        this.Scene3D?.NotifyTransformDirty(this);

        foreach (var child in this.Children)
        {
            child.PropagateWorldTransformDirty();
        }
    }

    /// <inheritdoc />
    internal override void PropagateWorldTransformDirty()
    {
        if (this.WorldTransformIsDirty) return;
        this._dirtyFlags |= DirtyFlags.WorldMatrix;
        this.Scene3D?.NotifyTransformDirty(this);

        base.PropagateWorldTransformDirty();
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

    /// <inheritdoc />
    public override void UpdateTransform(ref Matrix4x4 parentMatrix)
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
        Matrix4x4 mat = this.Parent?.WorldMatrix ?? Matrix4x4.Identity;
        this.UpdateTransform(ref mat);
    }

    /// <inheritdoc />
    public override Matrix4x4 WorldMatrix
    {
        get
        {
            if (this.WorldTransformIsDirty)
                this.UpdateTransform();
            return this._worldMatrix;
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

    /// <inheritdoc />
    public override void Mount(Scene3D scene)
    {
        base.Mount(scene);
        scene.NotifyTransformDirty(this);
    }

    /// <inheritdoc />
    public override void Unmount()
    {
        if (this.Scene3D == null) return;

        this.Scene3D.NotifyTransformNotDirty(this);
        base.Unmount();
    }

    /// <inheritdoc />
    protected override void AppendFormat(StringBuilder sb, char format)
    {
        switch (format)
        {
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
                base.AppendFormat(sb, format);
                break;
        }
    }
}
