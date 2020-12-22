using System;
using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Rendering
{
    public class Transform
    {
        public readonly Node3D node;

        private Vector3    _position;
        private Quaternion _rotation;
        private Vector3    _scale;

        public Vector3    Position { get => _position; set { _position = value; this._localMatrixDirty = true; TransformChanged?.Invoke(); } }
        public Quaternion Rotation { get => _rotation; set { _rotation = value; this._localMatrixDirty = true; TransformChanged?.Invoke(); } }
        public Vector3    Scale    { get => _scale;    set { _scale = value;    this._localMatrixDirty = true; TransformChanged?.Invoke(); } }

        public event Action? TransformChanged;

        public Vector3 Forward => Vector3.Transform(-Vector3.UnitZ, _rotation);
        
        private Matrix4x4 _localMatrix = Matrix4x4.Identity;
        private bool _localMatrixDirty = false;
        
        private Matrix4x4 _worldMatrix;
        public ref Matrix4x4 worldMatrix => ref this._worldMatrix;
        
        private List<Transform> _children = new List<Transform>();
        public IReadOnlyList<Transform> children => this._children;
        
        public void Add(Transform node)
        {
            this._children.Add(node);
        }

        public Transform(Node3D node)
        {
            this._position = Vector3.Zero;
            this._rotation = Quaternion.Identity;
            this._scale = Vector3.One;
            this.node = node;
        }

        public void UpdateWorldMatrix(ref Matrix4x4 parentMatrix)
        {
            this._worldMatrix = Matrix4x4.Multiply(this.GetLocalMatrix(), parentMatrix);
            for(int i = 0; i < this._children.Count; i++) {
                this._children[i].UpdateWorldMatrix(ref this._worldMatrix);
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