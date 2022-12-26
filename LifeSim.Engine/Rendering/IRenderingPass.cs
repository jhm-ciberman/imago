using System;
using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine;

public interface IRenderingPass : IDisposable
{
    void Render(CommandList cl, Scene scene);
}
