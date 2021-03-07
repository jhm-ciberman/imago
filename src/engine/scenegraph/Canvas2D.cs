using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class Canvas2D : ILayer
    {
        public Viewport viewport;

        private Node2D _root = new Node2D();
        public Node2D root => this._root;
        
        public Canvas2D(Viewport viewport)
        {
            this.viewport = viewport;
        }

        public void Add(Node2D node)
        {
            this._root.Add(node);
        } 

        public void Remove(Node2D node)
        {
            this._root.Remove(node);
        } 

        public void UpdateWorldMatrices()
        {
            foreach (var child in this._root.children) {
                child.UpdateWorldMatrix();
            }
        }

        void ILayer.Render(GPURenderer renderer)
        {
            renderer.RenderCanvas2D(this);
        }
    }
}