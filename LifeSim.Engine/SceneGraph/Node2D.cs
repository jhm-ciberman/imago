using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Engine.SceneGraph;

public class Node2D
{
    public string Name { get; set; } = string.Empty;

    private CanvasLayer? _canvas = null;

    public Node2D? Parent { get; private set; } = null;

    private readonly List<Node2D> _children = new List<Node2D>();
    public IReadOnlyList<Node2D> Children => this._children;

    private Vector2 _position = Vector2.Zero;
    private float _rotation = 0f;
    private Vector2 _scale = Vector2.One;

    public Vector2 Position { get => this._position; set { this._position = value; this.OnTransformDirty(); } }
    public float Rotation { get => this._rotation; set { this._rotation = value; this.OnTransformDirty(); } }
    public Vector2 Scale { get => this._scale; set { this._scale = value; this.OnTransformDirty(); } }

    private Matrix3x2 _localMatrix = Matrix3x2.Identity;
    private bool _localMatrixDirty = false;

    public bool Visible = true;

    private Matrix3x2 _worldMatrix;

    public Node2D()
    {
        //
    }

    public ref Matrix3x2 WorldMatrix => ref this._worldMatrix;

    public void Add(Node2D node)
    {
        if (node.Parent == this) return;
        if (node == this) return;

        node.Parent?.Remove(node);

        node._canvas = this._canvas;
        node.Parent = this;
        this._children.Add(node);
        this._canvas?.AddNodeToRecursive(node);
    }

    public void Remove(Node2D node)
    {
        if (node.Parent != this) return;

        this._children.Remove(node);
        node._canvas = null;
        node.Parent = null;
        this._canvas?.RemoveNodeRecursive(node);
    }

    protected void OnTransformDirty()
    {
        if (this._localMatrixDirty) return;
        this._localMatrixDirty = true;
    }

    public void UpdateWorldMatrix()
    {
        this._worldMatrix = this.GetLocalMatrix();
        for (int i = 0; i < this.Children.Count; i++)
        {
            this.Children[i].UpdateWorldMatrix(ref this._worldMatrix);
        }
    }

    private void UpdateWorldMatrix(ref Matrix3x2 parentMatrix)
    {
        this._worldMatrix = this.GetLocalMatrix() * parentMatrix;
        for (int i = 0; i < this.Children.Count; i++)
        {
            this.Children[i].UpdateWorldMatrix(ref this._worldMatrix);
        }
    }

    public ref Matrix3x2 GetLocalMatrix()
    {
        if (this._localMatrixDirty)
        {
            this._localMatrix = Matrix3x2.CreateScale(this._scale)
                * Matrix3x2.CreateRotation(this._rotation)
                * Matrix3x2.CreateTranslation(this._position);
            this._localMatrixDirty = false;
        }
        return ref this._localMatrix;
    }
}