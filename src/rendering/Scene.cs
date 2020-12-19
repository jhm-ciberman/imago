using System.Collections.Generic;

namespace LifeSim.Rendering
{
    public class Scene
    {
        private List<Renderable> _renderables = new List<Renderable>();
        private List<Camera> _cameras = new List<Camera>();

        public Scene()
        {
            // 
        }

        public void Add(Renderable renderable)
        {
            this._renderables.Add(renderable);
        }

        public void Add(Camera camera)
        {
            this._cameras.Add(camera);
        }

        public IReadOnlyList<Renderable> renderables => this._renderables;
        public IReadOnlyList<Camera> cameras => this._cameras;
    }
}