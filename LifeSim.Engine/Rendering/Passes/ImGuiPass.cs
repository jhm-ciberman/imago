using System;
using ImGuiNET;
using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine.Rendering.Passes;

public class ImGuiPass : IDisposable, IRenderingPass
{
    /// <summary>
    /// Gets the instance of the ImGuiPass.
    /// </summary>
    public static ImGuiPass Instance { get; private set; } = null!;

    private readonly GraphicsDevice _gd;
    private readonly ImGuiRenderer _imguiRenderer;
    private readonly IRenderTexture _renderTexture;

    public ImGuiPass(Renderer renderer, IRenderTexture renderTexture)
    {
        if (Instance != null)
            throw new InvalidOperationException("Only one instance of ImGuiPass can exist at a time.");

        Instance = this;

        this._gd = renderer.GraphicsDevice;
        this._renderTexture = renderTexture;
        this._imguiRenderer = new ImGuiRenderer(this._gd, renderTexture.OutputDescription, (int)renderTexture.Width, (int)renderTexture.Height);
        unsafe
        {
            ImGui.GetIO().NativePtr->IniFilename = null;
        }

        renderer.ViewportResized += this.OnViewportResized;
    }

    private void OnViewportResized(object? sender, ViewportResizedEventArgs e)
    {
        this._imguiRenderer.WindowResized((int)e.Width, (int)e.Height);
    }

    public void Dispose()
    {
        this._imguiRenderer.Dispose();
    }

    public void Update(float deltaTime, InputSnapshot inputSnapshot)
    {
        this._imguiRenderer.Update(deltaTime, inputSnapshot);
    }

    public IntPtr GetOrCreateBinding(Texture texture)
    {
        return this._imguiRenderer.GetOrCreateImGuiBinding(this._gd.ResourceFactory, texture.VeldridTexture);
    }

    public void Render(CommandList cl, Scene scene)
    {
        cl.SetFramebuffer(this._renderTexture.Framebuffer);
        this._imguiRenderer.Render(this._gd, cl);
    }
}
