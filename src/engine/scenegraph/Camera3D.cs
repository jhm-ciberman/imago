using System.Numerics;
using Veldrid;

namespace LifeSim.Engine.SceneGraph
{
    public class Camera3D
    {
        public Vector3 position;
        public Quaternion rotation = Quaternion.Identity;

        public float fov = 60 * System.MathF.PI / 180f;
        public float aspect => (float) this._viewPort.width / (float) this._viewPort.height;
        public float near = 0.01f;
        public float far = 1000.0f;

        private Viewport _viewPort;

        private RgbaFloat _clearColor = new RgbaFloat(0.84f, 0.84f, 0.86f, 1.0f);
        //private RgbaFloat _clearColor = new RgbaFloat(0.04f, 0.04f, 0.06f, 1.0f);
        public RgbaFloat clearColor => this._clearColor;

        public Viewport viewport => this._viewPort;
 
        public Camera3D(Viewport viewport)
        {
            this._viewPort = viewport;
        }

        public Matrix4x4 viewProjectionMatrix => this.viewMatrix * this.projectionMatrix;

        public Matrix4x4 projectionMatrix
        {
            get => Matrix4x4.CreatePerspectiveFieldOfView(this.fov, this.aspect, this.near, this.far);
        }

        public Matrix4x4 viewMatrix
        {
            get
            {
                Matrix4x4 worldMatrix = Matrix4x4.CreateFromQuaternion(this.rotation) * Matrix4x4.CreateTranslation(this.position);
                Matrix4x4.Invert(worldMatrix, out worldMatrix);
                return worldMatrix;
            }
        }

        public void LookAt(Vector3 destPoint)
        {
            Matrix4x4 worldMat = Matrix4x4.CreateWorld(this.position, destPoint - this.position, Vector3.UnitY);
            this.rotation = Quaternion.CreateFromRotationMatrix(worldMat);
        }
    }
}