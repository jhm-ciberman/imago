using System;
using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Rendering 
{
    public class Node3D
    {
        private Vector3    _position;
        private Quaternion _rotation;
        private Vector3    _scale;

        public Vector3    position { get => _position; set { _position = value; this._localMatrixDirty = true; TransformChanged?.Invoke(); } }
        public Quaternion rotation { get => _rotation; set { _rotation = value; this._localMatrixDirty = true; TransformChanged?.Invoke(); } }
        public Vector3    scale    { get => _scale;    set { _scale = value;    this._localMatrixDirty = true; TransformChanged?.Invoke(); } }

        public event Action? TransformChanged;

        private Node3D? _parent = null;
        public Node3D? parent => this._parent;

        public Vector3 forward => Vector3.Transform(Vector3.UnitZ, _rotation);
        
        private Matrix4x4 _localMatrix = Matrix4x4.Identity;
        private bool _localMatrixDirty = false;
        
        private Matrix4x4 _worldMatrix;
        public ref Matrix4x4 worldMatrix => ref this._worldMatrix;
        
        private List<Node3D> _children = new List<Node3D>();
        public IReadOnlyList<Node3D> children => this._children;
        
        public Node3D()
        {
            this._position = Vector3.Zero;
            this._rotation = Quaternion.Identity;
            this._scale = Vector3.One;
        }

        public void Add(Node3D node)
        {
            if (this._parent == this) return;
            node.parent?.Remove(node);
            if (! this._children.Contains(node)) {
                this._children.Add(node);
                node._parent = this;
            }
        }

        public void Remove(Node3D node)
        {
            if (node.parent == this) {
                this._children.Remove(node);
                node._parent = null;
            }
        }

        public void UpdateWorldMatrix()
        {
            this._worldMatrix = this.GetLocalMatrix();
            for(int i = 0; i < this._children.Count; i++) {
                this._children[i]._UpdateWorldMatrix(ref this._worldMatrix);
            }
        }

        internal void SetLocalMatrix(Matrix4x4 mat)
        {
            this._localMatrix = mat; // TODO: WTF
            this._localMatrixDirty = false;
        }

        private void _UpdateWorldMatrix(ref Matrix4x4 parentMatrix)
        {
            this._worldMatrix = this.GetLocalMatrix() * parentMatrix;
            for(int i = 0; i < this._children.Count; i++) {
                this._children[i]._UpdateWorldMatrix(ref this._worldMatrix);
            }
        }

        public ref Matrix4x4 GetLocalMatrix()
        {
            if (this._localMatrixDirty) {
                this._localMatrix = Matrix4x4.CreateScale(this._scale)
                    * Matrix4x4.CreateFromQuaternion(this._rotation)
                    * Matrix4x4.CreateTranslation(this._position);
                this._localMatrixDirty = false;
            }
            return ref this._localMatrix;
        }
    }
}