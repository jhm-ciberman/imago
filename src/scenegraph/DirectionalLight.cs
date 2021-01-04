using System.Numerics;

namespace LifeSim.SceneGraph
{
    public class DirectionalLight
    {
        public Vector3 position = new Vector3(100, 200, 100);
        public Vector3 color = new Vector3(1f, 1f, 1f);

        public Matrix4x4 shadowMapMatrix 
        {
            get
            {
                return Matrix4x4.CreateLookAt(this.position, new Vector3(0, 0, 0), Vector3.UnitY)
                    * Matrix4x4.CreateOrthographic(6, 6, 2f, 100f);
            }
        }
    }
}