using System.Numerics;
using Veldrid;

namespace LifeSim.Rendering
{
    public class Camera : System.IDisposable
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

        private float _near = 0.01f;
        public float near { get => this._near; set {this._near = value; this._projectionMatrixDirty = true;}}

        private float _far = 100.0f;
        public float far { get => this._far; set {this._far = value; this._projectionMatrixDirty = true;}}

        private DeviceBuffer _viewBuffer;
        private DeviceBuffer _projectionBuffer;
        private ResourceSet _projectionViewSet;
        public ResourceSet projectionViewSet => this._projectionViewSet;
        
        public Camera(ResourceFactory factory, ResourceLayout projectionViewLayout)
        {

            this._viewBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            this._projectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

            this._projectionViewSet = factory.CreateResourceSet(
                new ResourceSetDescription(projectionViewLayout, this._projectionBuffer, this._viewBuffer)
            );
        }

        public void Dispose()
        {
            this._viewBuffer.Dispose();
            this._projectionBuffer.Dispose();
            this._projectionViewSet.Dispose();
        }

        ~Camera() {
            this.Dispose();
        }

        public void UpdateMatrices(GraphicsDevice device)
        {
            if (this._projectionMatrixDirty) {
                this._projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(this._fov, this._aspect, this._near, this._far);
                //this._projectionMatrix = Matrix4x4.CreatePerspective(1,1, this._near, this._far);
                //this._projectionMatrix = Matrix4x4.CreateOrthographic(2, 2,  0.01f, 10f);
                this._projectionMatrixDirty = false;
                device.UpdateBuffer(this._projectionBuffer, 0, ref this._projectionMatrix);
            }

            if (this._viewMatrixDirty) {
                this._viewMatrix = Matrix4x4.CreateLookAt(this._position, this._lookAt, Vector3.UnitY);
                this._viewMatrixDirty = false;
                device.UpdateBuffer(this._viewBuffer, 0, ref this._viewMatrix);
            }
        }
    }
}