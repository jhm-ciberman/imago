using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Engine.SceneGraph
{
    public class Scene3D : Container<Node3D>
    {
        private List<Camera3D> _cameras = new List<Camera3D>();

        public DirectionalLight mainLight = new DirectionalLight();
        
        public Vector3 ambientColor = new Vector3(.2f, .2f, .2f);

        public Scene3D()
        {
            //
        }

        public void Add(Camera3D camera)
        {
            this._cameras.Add(camera);
        }

        public IReadOnlyList<Camera3D> cameras => this._cameras;

        public void UpdateWorldMatrices()
        {
            foreach (var child in this.children) {
                child.UpdateWorldMatrix();
            }
        }
    }
}