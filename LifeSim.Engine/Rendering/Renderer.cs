using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using LifeSim.Engine.SceneGraph;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace LifeSim.Engine.Rendering
{
    public class Renderer : System.IDisposable
    {
        public readonly IRenderTexture fullScreenRenderTexture;
        public readonly RenderTexture mainRenderTexture;
        public ResourceFactory assetManager => this._assetManager;
        public GraphicsBackend backendType => this._gd.BackendType;
        
        private readonly GraphicsDevice _gd;
        private readonly Veldrid.ResourceFactory _factory;
        private readonly CanvasRenderer _canvasRenderer;
        private readonly SceneRenderer _sceneRenderer;
        private readonly ImguiRenderer _imguiRenderer;
        private readonly FullScreenRenderer _fullScreenQuad;
        private readonly MousePickingRenderer _mousePicker;
        private readonly ResourceFactory _assetManager;
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

            this._factory = this._gd.ResourceFactory;



            this.fullScreenRenderTexture = new SwapchainRenderTexture(this._gd.MainSwapchain);
            this.mainRenderTexture = new RenderTexture(this._gd.ResourceFactory, (uint) window.Width, (uint) window.Height);

            this._canvasRenderer = new CanvasRenderer(this._gd, this.mainRenderTexture);
            this._sceneRenderer  = new SceneRenderer(this._gd, this.mainRenderTexture);
            this._imguiRenderer  = new ImguiRenderer(this._gd, this.mainRenderTexture);
            this._mousePicker    = new MousePickingRenderer(this._gd);
            this._fullScreenQuad = new FullScreenRenderer(this._gd, this.mainRenderTexture, this.fullScreenRenderTexture);

            this._fence = this._factory.CreateFence(false);
            this._assetManager = new ResourceFactory(this._gd, this._sceneRenderer);
        }

        public uint selectedObjectID => this._mousePicker.objectID;

        public Vector2 mousePickingPosition = Vector2.Zero;

        public void Update(float deltaTime, InputSnapshot inputSnapshot)
        {
            this._imguiRenderer.Update(deltaTime, inputSnapshot);
        }

        public IntPtr GetImGUITexture(Texture texture)
        {
            return this._imguiRenderer.Texture(texture);
        }

        private readonly List<Task> _renderTasks = new List<Task>();

        private bool _renderImGUI = false;

        public void RenderScene3D(Scene3D scene, Camera3D camera)
        {
            if (scene == null) return;
            //this._renderTasks.Add(Task.Run(() => {
                this._sceneRenderer.Render(scene, camera);
            //}));
        }

        public void RenderCanvas2D(Canvas2D canvas)
        {
            if (canvas == null) return;
            //this._renderTasks.Add(Task.Run(() => {
                this._canvasRenderer.Render(canvas);
            //}));
        }

        public void RenderImGUI(ImGUILayer layer)
        {
            this._renderImGUI = true;
            layer.OnGUI();
        }

        private Stopwatch _stopwatch = new Stopwatch();

        public void Render(IStage stage)
        {
            this._renderTasks.Clear();
            this._renderImGUI = false;
            stage.RenderFrame(this);

            //this._renderTasks.Add(Task.Run(() => {
                this._mousePicker.Update(this.mainRenderTexture, this.mousePickingPosition);
                if (this._renderImGUI) this._imguiRenderer.Render();
                this._fullScreenQuad.Render();
            //}));

            //Task.WaitAll(this._renderTasks.ToArray());
            this.WaitForGPU();

            this._sceneRenderer.Submit();
            this._canvasRenderer.Submit();
            this._mousePicker.Submit();
            if (this._renderImGUI) this._imguiRenderer.Submit();
            this._fullScreenQuad.Submit(this._fence);
            this._gd.SwapBuffers();
        }

        private void WaitForGPU()
        {
            if (! this._fence.Signaled) {
                // If we are GPU bound, then maybe it's a good moment to do a GC :)
                this._fence.Reset();
                GC.Collect(0, GCCollectionMode.Optimized);
            }
            this._gd.WaitForIdle();
        }

        internal void Resize(uint width, uint height, uint viewportWidth, uint viewportHeight)
        {
            this._gd.ResizeMainWindow(width, height);
            this._gd.WaitForIdle();
            this.fullScreenRenderTexture.Resize(width, height);
            this.mainRenderTexture.Resize(viewportWidth, viewportHeight);
            this._imguiRenderer.Resize(viewportWidth, viewportHeight);
        }

        public void Dispose()
        {
            this._gd.Dispose();
        }
    }
}