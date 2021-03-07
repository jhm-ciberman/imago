using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Engine.SceneGraph
{
    public class Node2D
    {
        public event System.Action<Node2D>? onTransformChanged;
        public event System.Action<Node2D>? onChildAdded;
        public event System.Action<Node2D>? onChildRemoved;

        public string name = string.Empty;

        private Node2D? _parent = null;
        public Node2D? parent => this._parent;

        private readonly List<Node2D> _children = new List<Node2D>();
        public IReadOnlyList<Node2D> children => this._children;

        private Vector2    _position;
        private float      _rotation;
        private Vector2    _scale;

        public Vector2    position { get => this._position; set { this._position = value; this._localMatrixDirty = true; } }
        public float      rotation { get => this._rotation; set { this._rotation = value; this._localMatrixDirty = true; } }
        public Vector2    scale    { get => this._scale;    set { this._scale = value;    this._localMatrixDirty = true; } }

        private Matrix3x2 _localMatrix = Matrix3x2.Identity;
        private bool _localMatrixDirty = false;

        public bool visible = true;
        
        private Matrix3x2 _worldMatrix;
        public ref Matrix3x2 worldMatrix => ref this._worldMatrix;

        public void Add(Node2D node)
        {
            if (node._parent == this) return;
            if (node == this) return;
            
            if (node._parent != null) {
                node._parent.Remove(node);
            }

            this._children.Add(node);
            node._parent = this;
            node.onChildAdded += this._OnNodeAdded;
            node.onChildRemoved += this._OnNodeRemoved;
            this.onChildAdded?.Invoke(node);
        }

        public void Remove(Node2D node)
        {
            if (node._parent != this) return;

            this._children.Remove(node);
            node._parent = null;
            node.onChildAdded -= this._OnNodeAdded;
            node.onChildRemoved -= this._OnNodeRemoved;
            this.onChildRemoved?.Invoke(node);
        }

        private void _OnNodeAdded(Node2D node)
        {
            this.onChildAdded?.Invoke(node);
        }

        private void _OnNodeRemoved(Node2D node)
        {
            this.onChildRemoved?.Invoke(node);
        }

        public void UpdateWorldMatrix()
        {
            this._worldMatrix = this.GetLocalMatrix();
            for(int i = 0; i < this.children.Count; i++) {
                this.children[i]._UpdateWorldMatrix(ref this._worldMatrix);
            }
        }

        private void _UpdateWorldMatrix(ref Matrix3x2 parentMatrix)
        {
            this._worldMatrix = this.GetLocalMatrix() * parentMatrix;
            for(int i = 0; i < this.children.Count; i++) {
                this.children[i]._UpdateWorldMatrix(ref this._worldMatrix);
            }
        }

        public ref Matrix3x2 GetLocalMatrix()
        {
            if (this._localMatrixDirty) {
                this._localMatrix = Matrix3x2.CreateScale(this._scale)
                    * Matrix3x2.CreateRotation(this._rotation)
                    * Matrix3x2.CreateTranslation(this._position);
                this._localMatrixDirty = false;
            }
            return ref this._localMatrix;
        }
    }
}