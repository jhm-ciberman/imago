using System.Numerics;
using LifeSim.Rendering;
using Veldrid.Utilities;

namespace LifeSim.Engine.SceneGraph
{
    public class Camera3D : ICamera
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Quaternion Rotation = Quaternion.Identity;

        public float FieldOfView = 60 * System.MathF.PI / 180f;
        public float Aspect => (float) this.Viewport.Width / (float) this.Viewport.Height;
        public float Near = 0.01f;
        public float Far = 100.0f;

        public Viewport Viewport { get; set; }

        public ICamera FrustumCullingCamera { get; private set; }

        public BoundingFrustum FrustumForCulling => new BoundingFrustum(this.ViewProjectionMatrix);
 
        public Camera3D(Viewport viewport)
        {
            this.Viewport = viewport;
            this.FrustumCullingCamera = this;
        }

        public Matrix4x4 ViewProjectionMatrix => this.ViewMatrix * this.ProjectionMatrix;

        public Matrix4x4 ProjectionMatrix => Matrix4x4.CreatePerspectiveFieldOfView(this.FieldOfView, this.Aspect, this.Near, this.Far);

        public Matrix4x4 ViewMatrix
        {
            get
            {
                Matrix4x4 worldMatrix = Matrix4x4.CreateFromQuaternion(this.Rotation) * Matrix4x4.CreateTranslation(this.Position);
                _ = Matrix4x4.Invert(worldMatrix, out worldMatrix);
                return worldMatrix;
            }
        }

        public void LookAt(Vector3 destPoint)
        {
            Matrix4x4 worldMat = Matrix4x4.CreateWorld(this.Position, destPoint - this.Position, Vector3.UnitY);
            this.Rotation = Quaternion.CreateFromRotationMatrix(worldMat);
        }
    }
}