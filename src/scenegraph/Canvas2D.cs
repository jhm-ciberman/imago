namespace LifeSim.SceneGraph
{
    public class Canvas2D : Container2D
    {
        public Viewport viewport;

        public Canvas2D(Viewport viewport)
        {
            this.viewport = viewport;
        }
    }
}