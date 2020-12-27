using System.Collections.Generic;

namespace LifeSim.SceneGraph
{
    public class Scene3D : Container3D
    {
        private List<Camera3D> _cameras = new List<Camera3D>();

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