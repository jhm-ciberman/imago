using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public interface ILayer
    {
        void Render(GPURenderer renderer);
    }
}