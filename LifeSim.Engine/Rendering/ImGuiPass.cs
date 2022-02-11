using System;
using ImGuiNET;
using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class ImGuiPass : IDisposable, IRenderingPass
{
    private readonly GraphicsDevice _gd;
    private readonly Veldrid.ImGuiRenderer _imguiRenderer;
    private readonly IRenderTexture _renderTexture;

    public ImGuiPass(Renderer renderer, IRenderTexture renderTexture)
    {
        this._gd = renderer.GraphicsDevice;
        this._renderTexture = renderTexture;
        this._imguiRenderer = new ImGuiRenderer(this._gd, renderTexture.OutputDescription, (int)renderTexture.Width, (int)renderTexture.Height);
        unsafe
        {
            ImGui.GetIO().NativePtr->IniFilename = null;
        }
    }

    public void Dispose()
    {
        this._imguiRenderer.Dispose();
    }

    public void Update(float deltaTime, InputSnapshot inputSnapshot)
    {
        this._imguiRenderer.Update(deltaTime, inputSnapshot);
    }

    public void Resize(uint width, uint height)
    {
        this._imguiRenderer.WindowResized((int)width, (int)height);
    }

    public IntPtr GetOrCreateBinding(Texture texture)
    {
        return this._imguiRenderer.GetOrCreateImGuiBinding(this._gd.ResourceFactory, texture.DeviceTexture);
    }

    public void Render(CommandList cl, Scene scene)
    {
        this._imguiRenderer.Render(this._gd, cl);
    }
}