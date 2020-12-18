using System.Numerics;

namespace LifeSim.Rendering
{
    public class Camera
    {
        private Matrix4x4 _viewMatrix;
        private bool _viewMatrixDirty = true;
        
        private Matrix4x4 _projectionMatrix;
        private bool _projectionMatrixDirty = true;

        private Vector3 _position;
        public Vector3 position { get => this._position; set {this._position = value; this._viewMatrixDirty = true;}}

        private Vector3 _lookAt = Vector3.UnitZ;
        public Vector3 lookAt { get => this._lookAt; set {this._lookAt = value; this._viewMatrixDirty = true;}}

        private float _fov = 60 * System.MathF.PI / 180f;
        public float fov { get => this._fov; set {this._fov = value; this._projectionMatrixDirty = true;}}

        private float _aspect = 4f / 3f;
        public float aspect { get => this._aspect; set {this._aspect = value; this._projectionMatrixDirty = true;}}

        private float _near = 0.09f;
        public float near { get => this._near; set {this._near = value; this._projectionMatrixDirty = true;}}

        private float _far = 10.0f;
        public float far { get => this._far; set {this._far = value; this._projectionMatrixDirty = true;}}

        public Camera()
        {
            //
        }

        public void UpdateMatrices()
        {
            if (this._projectionMatrixDirty) {
                this._projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(this._fov, this._aspect, this._near, this._far);
                //this._projectionMatrix = Matrix4x4.CreatePerspective(1,1, this._near, this._far);
                //this._projectionMatrix = Matrix4x4.CreateOrthographic(2, 2,  0.01f, 10f);
                this._projectionMatrixDirty = false;
            }

            if (this._viewMatrixDirty) {
                this._viewMatrix = Matrix4x4.CreateLookAt(this._position, this._lookAt, Vector3.UnitY);
                this._viewMatrixDirty = false;
            }
        }

        public ref Matrix4x4 projectionMatrix => ref this._projectionMatrix;
        public ref Matrix4x4 viewMatrix => ref this._viewMatrix;
    }
}