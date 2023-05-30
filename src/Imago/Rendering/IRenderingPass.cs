using System;
using Imago.SceneGraph;
using Veldrid;

namespace Imago.Rendering;

public interface IRenderingPass : IDisposable
{
    void Render(CommandList cl, Scene scene);
}
