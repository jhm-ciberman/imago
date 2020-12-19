using System.Collections.Generic;

namespace LifeSim.Rendering
{
    public class Scene
    {
        private List<Renderable> _renderables = new List<Renderable>();
        private Camera _camera;

        public Scene(Camera camera)
        {
            this._camera = camera;
        }

        public void Add(Renderable renderable)
        {
            this._renderables.Add(renderable);
        }

        public IReadOnlyList<Renderable> renderables => this._renderables;
        public Camera camera => this._camera;
    }
}