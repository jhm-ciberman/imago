using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Engine.SceneGraph
{
    public class Node2D
    {
        public string name {get; set;} = string.Empty;

        private Canvas2D? _canvas = null;

        private Node2D? _parent = null;
        public Node2D? parent => this._parent;

        private readonly List<Node2D> _children = new List<Node2D>();
        public IReadOnlyList<Node2D> children => this._children;

        private Vector2    _position;
        private float      _rotation;
        private Vector2    _scale;

        public Vector2    position { get => this._position; set { this._position = value; this._OnTransformDirty(); } }
        public float      rotation { get => this._rotation; set { this._rotation = value; this._OnTransformDirty(); } }
        public Vector2    scale    { get => this._scale;    set { this._scale = value;    this._OnTransformDirty(); } }

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

            node._canvas = this._canvas;
            node._parent = this;
            this._children.Add(node);
            this._canvas?._AddNodeToRecursive(node);
        }

        public void Remove(Node2D node)
        {
            if (node._parent != this) return;

            this._children.Remove(node);
            node._canvas = null;
            node._parent = null;
            this._canvas?._RemoveNodeRecursive(node);
        }

        private void _OnTransformDirty()
        {
            if (this._localMatrixDirty) return;
            this._localMatrixDirty = true;
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