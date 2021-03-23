using System.Numerics;

namespace LifeSim.Engine.SceneGraph
{
    public class DirectionalLight
    {
        public Vector3 direction = new Vector3(100, 200, 100);
        public ColorF color = ColorF.white;

        public Vector2 shadowMapSize = new Vector2(20f, 20f);
        public float shadowZNear = 2f;
        public float shadowZFar = 100f;
        
        public Matrix4x4 GetShadowMapMatrix(Vector3 cameraPosition)
        {
            return Matrix4x4.CreateLookAt(cameraPosition + this.direction, cameraPosition, Vector3.UnitY)
                * Matrix4x4.CreateOrthographic(this.shadowMapSize.X, this.shadowMapSize.Y, this.shadowZNear, this.shadowZFar);
        }

        
    }
}