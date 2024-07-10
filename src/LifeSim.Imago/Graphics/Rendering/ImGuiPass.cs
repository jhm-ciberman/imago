using System;
using ImGuiNET;
using LifeSim.Imago.Graphics.Textures;
using Veldrid;
using Texture = LifeSim.Imago.Graphics.Textures.Texture;

namespace LifeSim.Imago.Graphics.Rendering;

public class ImGuiPass : IDisposable
{
    private readonly GraphicsDevice _gd;
    private readonly ImGuiRenderer _imguiRenderer;

    public ImGuiPass(Renderer renderer)
    {
        this._gd = renderer.GraphicsDevice;
        var renderTexture = renderer.MainRenderTexture;
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

    public nint GetOrCreateBinding(Texture texture)
    {
        return this._imguiRenderer.GetOrCreateImGuiBinding(this._gd.ResourceFactory, texture.VeldridTexture);
    }

    public void Render(CommandList cl, RenderTexture renderTexture)
    {
        cl.SetFramebuffer(renderTexture.Framebuffer);
        this._imguiRenderer.Render(this._gd, cl);
    }
}
