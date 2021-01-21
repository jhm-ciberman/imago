using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public abstract class ImGUILayer : ILayer
    {
        public void Render(GPURenderer renderer)
        {
            renderer.RenderImGUI(this);
        }

        public abstract void OnGUI();
    }
}