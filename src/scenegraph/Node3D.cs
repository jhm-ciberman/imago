using System;
using System.Numerics;

namespace LifeSim.SceneGraph 
{
    public class Node3D : Container3D
    {
        private Vector3    _position;
        private Quaternion _rotation;
        private Vector3    _scale;

        public Vector3    position { get => _position; set { _position = value; this._localMatrixDirty = true; TransformChanged?.Invoke(); } }
        public Quaternion rotation { get => _rotation; set { _rotation = value; this._localMatrixDirty = true; TransformChanged?.Invoke(); } }
        public Vector3    scale    { get => _scale;    set { _scale = value;    this._localMatrixDirty = true; TransformChanged?.Invoke(); } }

        public event Action? TransformChanged;

        public Vector3 forward => Vector3.Transform(Vector3.UnitZ, _rotation);
        
        private Matrix4x4 _localMatrix = Matrix4x4.Identity;
        private bool _localMatrixDirty = false;
        
        private Matrix4x4 _worldMatrix;
        public ref Matrix4x4 worldMatrix => ref this._worldMatrix;
        
        public Node3D()
        {
            this._position = Vector3.Zero;
            this._rotation = Quaternion.Identity;
            this._scale = Vector3.One;
        }

        public void UpdateWorldMatrix()
        {
            this._worldMatrix = this.GetLocalMatrix();
            for(int i = 0; i < this.children.Count; i++) {
                this.children[i]._UpdateWorldMatrix(ref this._worldMatrix);
            }
        }

        private void _UpdateWorldMatrix(ref Matrix4x4 parentMatrix)
        {
            this._worldMatrix = this.GetLocalMatrix() * parentMatrix;
            for(int i = 0; i < this.children.Count; i++) {
                this.children[i]._UpdateWorldMatrix(ref this._worldMatrix);
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