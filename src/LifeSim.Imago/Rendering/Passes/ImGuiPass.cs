using System;
using ImGuiNET;
using LifeSim.Imago.Assets.Textures;
using Veldrid;
using Texture = LifeSim.Imago.Assets.Textures.Texture;

namespace LifeSim.Imago.Rendering.Passes;

internal class ImGuiPass : IDisposable
{
    private readonly GraphicsDevice _gd;
    private readonly ImGuiRenderer _imguiRenderer;
    private readonly IRenderTexture _renderTexture;

    public ImGuiPass(Renderer renderer)
    {
        this._gd = renderer.GraphicsDevice;
        this._renderTexture = renderer.FullScreenRenderTexture;
        this._imguiRenderer = new ImGuiRenderer(this._gd, this._renderTexture.OutputDescription, (int)this._renderTexture.Width, (int)this._renderTexture.Height);
        unsafe
        {
            ImGui.GetIO().NativePtr->IniFilename = null;
        }

        this._renderTexture.Resized += this.OnViewportResized;
    }

    private void OnViewportResized(object? sender, EventArgs e)
    {
        var renderTexture = (IRenderTexture)sender!;
        this._imguiRenderer.WindowResized((int)renderTexture.Width, (int)renderTexture.Height);
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

    public void Render(CommandList cl)
    {
        cl.SetFramebuffer(this._renderTexture.Framebuffer);
        this._imguiRenderer.Render(this._gd, cl);
    }
}
