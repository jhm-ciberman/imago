using System.Numerics;

namespace LifeSim.Engine.SceneGraph
{
    public class Node2D : Container2D
    {
        private Vector2    _position;
        private float      _rotation;
        private Vector2    _scale;

        public Vector2    position { get => _position; set { _position = value; this._localMatrixDirty = true; } }
        public float      rotation { get => _rotation; set { _rotation = value; this._localMatrixDirty = true; } }
        public Vector2    scale    { get => _scale;    set { _scale = value;    this._localMatrixDirty = true; } }

        private Matrix3x2 _localMatrix = Matrix3x2.Identity;
        private bool _localMatrixDirty = false;
        
        private Matrix3x2 _worldMatrix;
        public ref Matrix3x2 worldMatrix => ref this._worldMatrix;

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