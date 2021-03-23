using System;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine
{
    public interface IStage
    {
        void RenderFrame(GPURenderer renderer);
        void Update(float deltaTime);
    }
}