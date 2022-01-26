using System;
using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public interface IRenderer
{
    IRenderTexture FullScreenRenderTexture { get; }
    IRenderTexture MainRenderTexture { get; }
    GraphicsDevice GraphicsDevice { get; }
    GraphicsBackend BackendType { get; }
    IPipelineProvider ForwardPass { get; }
    IPipelineProvider ShadowMapPass { get; }
    SceneStorage Storage { get; }
    uint MousePickerObjectID { get; }
    ShadowMapConfig ShadowMapConfig { get; }
    ITexture ShadowMapTexture { get; }

    void Dispose();
    IntPtr GetOrCreateImGuiBinding(Texture texture);
    void Render(Scene scene, float deltaTime, InputSnapshot inputSnapshot);
    void Resize(uint width, uint height, uint viewportWidth, uint viewportHeight);
}
