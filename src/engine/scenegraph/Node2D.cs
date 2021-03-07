using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Engine.SceneGraph
{
    public class Node2D
    {
        public event System.Action<Event<Node2D>>? onEvent;

        public string name {get; set;} = string.Empty;

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

            this._children.Add(node);
            node._parent = this;
            node.onEvent += this._OnNotified;
            this._Notify(node, EventType.ChildAdded);
        }

        public void Remove(Node2D node)
        {
            if (node._parent != this) return;

            this._children.Remove(node);
            node._parent = null;
            node.onEvent -= this._OnNotified;
            this._Notify(node, EventType.ChildRemoved);
        }

        private void _OnNotified(Event<Node2D> e)
        {
            this.onEvent?.Invoke(e);
        }

        protected void _Notify(Node2D node, EventType eventType)
        {
            this.onEvent?.Invoke(new Event<Node2D>(node, eventType));
        }

        private void _OnTransformDirty()
        {
            if (this._localMatrixDirty) return;
            this._localMatrixDirty = true;
            this._Notify(this, EventType.TransformDirty);
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