using System;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace LifeSim.Rendering
{
    public class Renderer : IDisposable
    {
        // This is the only global variable! I swear!! 
        // Please don't point your finger at me with that face (?)
        public static GraphicsDevice GraphicsDevice = null!;

        public readonly IRenderTexture FullScreenRenderTexture;
        public readonly RenderTexture MainRenderTexture;
        private readonly GraphicsDevice _gd;
        private readonly ResourceFactory _factory;
        private readonly FullScreenRenderer _fullScreenRenderer;

        public GizmosRenderer GizmosRenderer { get; }

        public ParticlesRenderer ParticlesRenderer { get; }

        public SceneRenderer SceneRenderer { get; }

        public ImguiRenderer ImguiRenderer { get; }

        public CanvasRenderer CanvasRenderer { get; }

        public MousePickingRenderer MousePicker { get; }

        public SceneStorage SceneStorage => this.SceneRenderer.Storage;
        public GraphicsBackend BackendType => this._gd.BackendType;

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

            this.CanvasRenderer = new CanvasRenderer(this._gd, this.MainRenderTexture);
            this.SceneRenderer = new SceneRenderer(this._gd, this.MainRenderTexture);
            this.ImguiRenderer = new ImguiRenderer(this._gd, this.MainRenderTexture);
            this.MousePicker = new MousePickingRenderer(this._gd, this.MainRenderTexture);
            this.GizmosRenderer = new GizmosRenderer(this._gd, this.MainRenderTexture);
            this.ParticlesRenderer = new ParticlesRenderer(this._gd, this.MainRenderTexture);
            this._fullScreenRenderer = new FullScreenRenderer(this._gd, this.MainRenderTexture, this.FullScreenRenderTexture);

            this._fence = this._factory.CreateFence(false);
        }

        public void Render()
        {
            this.ImguiRenderer.Render();
            this._fullScreenRenderer.Render();
            

            this.WaitForGPU();

            this.SceneRenderer.Submit();
            this.ParticlesRenderer.Submit();
            this.GizmosRenderer.Submit();
            this.CanvasRenderer.Submit();
            this.MousePicker.Submit();
            this.ImguiRenderer.Submit();
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
            this.ImguiRenderer.Resize(viewportWidth, viewportHeight);
        }

        public void Dispose()
        {
            this.FullScreenRenderTexture.Dispose();
            this.MainRenderTexture.Dispose();
            this.CanvasRenderer.Dispose();
            this.SceneRenderer.Dispose();
            this.ImguiRenderer.Dispose();
            this.MousePicker.Dispose();
            this.GizmosRenderer.Dispose();
            this._fullScreenRenderer.Dispose();
            this._fence.Dispose();
            this._gd.Dispose();
        }
    }
}