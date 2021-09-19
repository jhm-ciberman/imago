using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace LifeSim.Rendering
{
    public class Renderer : System.IDisposable
    {
        // This is the only global variable! I swear!! 
        // Please don't point your finger at me with that face (?)
        public static GraphicsDevice GraphicsDevice = null!;

        public readonly IRenderTexture FullScreenRenderTexture;
        public readonly RenderTexture MainRenderTexture;
        private readonly GraphicsDevice _gd;
        private readonly ResourceFactory _factory;
        private readonly CanvasRenderer _canvasRenderer;
        private readonly SceneRenderer _sceneRenderer;
        private readonly ImguiRenderer _imguiRenderer;
        private readonly FullScreenRenderer _fullScreenRenderer;
        private readonly MousePickingRenderer _mousePicker;
        private readonly Fence _fence;

        public Renderer(Sdl2Window window, GraphicsBackend graphicsBackend)
        {
            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: false,
                resourceBindingModel: ResourceBindingModel.Default,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true,
                swapchainSrgbFormat: false
            );

            this._gd = VeldridStartup.CreateGraphicsDevice(window, options, graphicsBackend);
            Renderer.GraphicsDevice = this._gd;

            this._factory = this._gd.ResourceFactory;

            this.FullScreenRenderTexture = new SwapchainRenderTexture(this._gd.MainSwapchain);
            this.MainRenderTexture = new RenderTexture(this._gd.ResourceFactory, (uint) window.Width, (uint) window.Height);

            this._canvasRenderer = new CanvasRenderer(this._gd, this.MainRenderTexture);
            this._sceneRenderer  = new SceneRenderer(this._gd, this.MainRenderTexture);
            this._imguiRenderer  = new ImguiRenderer(this._gd, this.MainRenderTexture);
            this._mousePicker    = new MousePickingRenderer(this._gd);
            this._fullScreenRenderer = new FullScreenRenderer(this._gd, this.MainRenderTexture, this.FullScreenRenderTexture);

            this._fence = this._factory.CreateFence(false);
        }

        public SceneStorage SceneStorage => this._sceneRenderer.Storage;
        public GraphicsBackend BackendType => this._gd.BackendType;

        public void Update(float deltaTime, InputSnapshot inputSnapshot)
        {
            this._imguiRenderer.Update(deltaTime, inputSnapshot);
        }

        public IntPtr GetImGUITexture(Texture texture)
        {
            return this._imguiRenderer.Texture(texture);
        }

        public void RenderScene3D(IReadOnlyList<Renderable> renderable, DirectionalLight mainLight, ColorF ambientColor, ColorF clearColor, ICamera camera)
        {
            this._sceneRenderer.Render(renderable, mainLight, ambientColor, clearColor, camera);
        }

        public void RenderCanvas2D(Viewport viewport, IReadOnlyList<ICanvasItem> canvasItems)
        {
            this._canvasRenderer.Render(viewport, canvasItems);
        }

        public void UpdateMousePicking(Vector2 mousePickingPosition)
        {
            this._mousePicker.Update(this.MainRenderTexture, mousePickingPosition);
        }

        public uint SelectedObjectID => this._mousePicker.ObjectID;

        public void Render()
        {
            this._imguiRenderer.Render();
            this._fullScreenRenderer.Render();

            this.WaitForGPU();

            this._sceneRenderer.Submit();
            this._canvasRenderer.Submit();
            this._mousePicker.Submit();
            this._imguiRenderer.Submit();
            this._fullScreenRenderer.Submit(this._fence);
            this._gd.SwapBuffers();
        }

        public void WaitForGPU()
        {
            if (! this._fence.Signaled) { // If we are GPU bound, then maybe it's a good moment to do a GC :)
                this._fence.Reset();
                GC.Collect(0, GCCollectionMode.Optimized);
            }
            this._gd.WaitForIdle();
        }

        public void Resize(uint width, uint height, uint viewportWidth, uint viewportHeight)
        {
            this._gd.ResizeMainWindow(width, height);
            this._gd.WaitForIdle();
            this.FullScreenRenderTexture.Resize(width, height);
            this.MainRenderTexture.Resize(viewportWidth, viewportHeight);
            this._imguiRenderer.Resize(viewportWidth, viewportHeight);
        }

        public void Dispose()
        {
            this._gd.Dispose();
        }
    }
}