namespace LifeSim.Engine.SceneGraph
{
    public class Canvas2D : Container<Node2D>
    {
        public Viewport viewport;

        public Canvas2D(Viewport viewport)
        {
            this.viewport = viewport;
        }

        public void UpdateWorldMatrices()
        {
            foreach (var child in this.children) {
                child.UpdateWorldMatrix();
            }
        }
    }
}