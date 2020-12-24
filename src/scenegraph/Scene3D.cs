using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Rendering
{
    public class Scene3D : Node3D
    {
        private List<Camera> _cameras = new List<Camera>();

        public Scene3D()
        {
            //
        }

        public void Add(Camera camera)
        {
            this._cameras.Add(camera);
        }

        public IReadOnlyList<Camera> cameras => this._cameras;

        public void UpdateWorldMatrices()
        {
            foreach (var child in this.children) {
                child.UpdateWorldMatrix();
            }
        }
    }
}