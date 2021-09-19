using System.Numerics;

namespace LifeSim.Rendering
{
    public class DirectionalLight
    {
        public Vector3 Direction { get; set; } = new Vector3(100, 200, 100);
        public ColorF Color { get; set; } = ColorF.White;

        public Vector2 ShadowMapSize { get; set; } = new Vector2(20f, 20f);
        public float ShadowZNear { get; set; } = 2f;
        public float ShadowZFar { get; set; } = 100f;
        
        public Matrix4x4 GetShadowMapMatrix(Vector3 cameraPosition)
        {
            return Matrix4x4.CreateLookAt(cameraPosition + this.Direction, cameraPosition, Vector3.UnitY)
                * Matrix4x4.CreateOrthographic(this.ShadowMapSize.X, this.ShadowMapSize.Y, this.ShadowZNear, this.ShadowZFar);
        }
    }
}