using System;
using ImGuiNET;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class ImguiRenderer : IDisposable
    {
        private readonly CommandList _commandList;
        private readonly GraphicsDevice _gd;
        private readonly Veldrid.ImGuiRenderer _imguiRenderer;
        private readonly RenderTexture _renderTexture;

        public ImguiRenderer(GraphicsDevice graphicsDevice, RenderTexture renderTexture)
        {
            this._gd = graphicsDevice;
            this._commandList = graphicsDevice.ResourceFactory.CreateCommandList();
            this._renderTexture = renderTexture;
            this._imguiRenderer = new ImGuiRenderer(this._gd, renderTexture.outputDescription, (int) renderTexture.width, (int) renderTexture.height);
            unsafe {
                ImGui.GetIO().NativePtr->IniFilename = null;
            }
        }

        public void Dispose()
        {
            this._imguiRenderer.Dispose();
            this._commandList.Dispose();
        }

        public void Update(float deltaTime, InputSnapshot inputSnapshot)
        {
            this._imguiRenderer.Update(deltaTime, inputSnapshot);
        }

        public void Resize(uint width, uint height)
        {
            this._imguiRenderer.WindowResized((int) width, (int) height);
        }

        public IntPtr Texture(Texture texture)
        {
            return this._imguiRenderer.GetOrCreateImGuiBinding(this._gd.ResourceFactory, texture.deviceTexture);
        }

        public void Render()
        {
            this._commandList.Begin();
            this._commandList.SetFramebuffer(this._renderTexture.framebuffer);
            this._imguiRenderer.Render(this._gd, this._commandList);
            this._commandList.End();

        }

        public void Submit()
        {
            this._gd.SubmitCommands(this._commandList);
        }
    }
}