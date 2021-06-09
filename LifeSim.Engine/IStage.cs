using System;
using LifeSim.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine
{
    public interface IStage
    {
        void RenderFrame(Renderer renderer);
        void Update(float deltaTime);
    }
}