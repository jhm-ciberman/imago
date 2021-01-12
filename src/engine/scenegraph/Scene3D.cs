using System.Numerics;

namespace LifeSim.Engine.SceneGraph
{
    public class Scene3D : Container<Node3D>
    {
        public Camera3D? activeCamera = null;

        public DirectionalLight mainLight = new DirectionalLight();
        
        public Vector3 ambientColor = new Vector3(.2f, .2f, .2f);

        public Scene3D()
        {
            //
        }

        public void UpdateWorldMatrices()
        {
            foreach (var child in this.children) {
                child.UpdateWorldMatrix();
            }
        }
    }
}