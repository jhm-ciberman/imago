using System;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine
{
    public interface IScene
    {
        void RenderFrame(Renderer renderer);

        void RenderImGui();
        void Update(float deltaTime);
    }
}