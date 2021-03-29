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
    public class GPURenderer : System.IDisposable
    {
        private readonly GraphicsDevice _gd;
        private readonly Veldrid.ResourceFactory _factory;
        
        private readonly GPURenderer2D _renderer2d;
        private readonly GPURenderer3D _renderer3d;
        private readonly ImguiRenderer _imguiRenderer;

        private readonly FullScreenRenderer _fullScreenQuad;

        public GraphicsBackend backendType => this._gd.BackendType;

        private readonly GPUMousePicker _mousePicker;

        private readonly GPUResourceManager _gpuResources;

        private readonly ResourceFactory _assetManager;
        public ResourceFactory assetManager => this._assetManager;

        private readonly PSOManager _psoManager;

        public GPURenderer(Sdl2Window window, GraphicsBackend graphicsBackend)
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

            this._gpuResources = new GPUResourceManager(this._gd, (uint) window.Width, (uint) window.Height);

            this._psoManager = new PSOManager(this._factory);

            this._assetManager = new ResourceFactory(this._gd, this._gpuResources);

            this._renderer2d     = new GPURenderer2D(this._gd, this._assetManager, this._gpuResources, this._psoManager);
            this._renderer3d     = new GPURenderer3D(this._gd, this._psoManager, this._gpuResources);
            this._imguiRenderer  = new ImguiRenderer(this._gd, this._gpuResources.mainRenderTexture);
            this._mousePicker    = new GPUMousePicker(this._gd);
            this._fullScreenQuad = new FullScreenRenderer(this._gd, this._assetManager, this._psoManager, this._gpuResources);
        }

        public uint selectedObjectID => this._mousePicker.objectID;

        public Vector2 mousePickingPosition = Vector2.Zero;

        public FrameProfiler.FrameStats baseStats => this._renderer3d.frameProfilerBase.stats;
        public FrameProfiler.FrameStats shadowmapStats => this._renderer3d.frameProfilerShadowmap.stats;

        public void Update(float deltaTime, InputSnapshot inputSnapshot)
        {
            this._imguiRenderer.Update(deltaTime, inputSnapshot);
        }

        public IntPtr GetImGUITexture(GPUTexture texture)
        {
            return this._imguiRenderer.Texture(texture);
        }

        private readonly List<Task> _renderTasks = new List<Task>();

        private bool _renderImGUI = false;

        public void RenderScene3D(Scene3D scene, Camera3D camera)
        {
            if (scene == null) return;
            this._renderTasks.Add(Task.Run(() => {
                this._renderer3d.Render(scene, camera);
            }));
        }

        public void RenderCanvas2D(Canvas2D canvas)
        {
            if (canvas == null) return;
            this._renderTasks.Add(Task.Run(() => {
                this._renderer2d.Render(canvas);
            }));
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

            this._renderTasks.Add(Task.Run(() => {
                this._mousePicker.Update(this._gpuResources.mainRenderTexture, this.mousePickingPosition);
                if (this._renderImGUI) this._imguiRenderer.Render();
                this._fullScreenQuad.Render();
            }));

            Task.WaitAll(this._renderTasks.ToArray());
            
            this._gd.WaitForIdle();

            this._renderer3d.Submit();
            this._renderer2d.Submit();
            this._mousePicker.Submit();
            if (this._renderImGUI) this._imguiRenderer.Submit();
            this._fullScreenQuad.Submit();

            this._gd.SwapBuffers();
        }

        internal void Resize(uint width, uint height, uint viewportWidth, uint viewportHeight)
        {
            this._gd.ResizeMainWindow(width, height);
            this._gd.WaitForIdle();
            this._gpuResources.fullScreenRenderTexture.Resize(width, height);
            this._gpuResources.mainRenderTexture.Resize(viewportWidth, viewportHeight);
            this._imguiRenderer.Resize(viewportWidth, viewportHeight);
        }

        public void Dispose()
        {
            this._gd.Dispose();
        }
    }
}