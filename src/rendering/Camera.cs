using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace LifeSim.Rendering
{
    public class Camera
    {
        public Vector3 position;
        public Vector3 lookAt = Vector3.UnitZ;
        public float fov = 60 * System.MathF.PI / 180f;
        public float aspect = 4f / 3f;
        public float near = 0.01f;
        public float far = 100.0f;

 
        public Camera()
        {
            //
        }

        public Matrix4x4 projectionMatrix
        {
            get => Matrix4x4.CreatePerspectiveFieldOfView(this.fov, this.aspect, this.near, this.far);
        }

        public Matrix4x4 viewMatrix
        {
            get => Matrix4x4.CreateLookAt(this.position, this.lookAt, Vector3.UnitY);
        }
    }
}