using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.SceneGraph
{
    public class Scene3D : Container3D
    {
        private List<Camera3D> _cameras = new List<Camera3D>();

        public Vector3 sunPosition = new Vector3(1000, 2000, 1000);
        public Vector3 sunColor = new Vector3(1f, 1f, 1f);
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