using System.Numerics;
using System.Threading.Tasks;
using LifeSim.Engine.SceneGraph;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace LifeSim.Engine.Rendering
{
    public class GPURenderer : System.IDisposable
    {
        private GraphicsDevice _gd;
        private ResourceFactory _factory;
        
        private GPURenderer2D _renderer2d;
        private GPURenderer3D _renderer3d;
        private ImguiRenderer _imguiRenderer;

        private FullScreenRenderer _fullScreenQuad;

        public GraphicsBackend backendType => this._gd.BackendType;

        private GPUMousePicker _mousePicker;

        private GPUResourceManager _gpuResources;

        private AssetManager _assetManager;
        public AssetManager assetManager => this._assetManager;

        private PSOManager _psoManager;

        public GPURenderer(Sdl2Window window, GraphicsBackend graphicsBackend)
        {
            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: true,
                resourceBindingModel: ResourceBindingModel.Default,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true
            );

            this._gd = VeldridStartup.CreateGraphicsDevice(window, options, graphicsBackend);
            this._factory = this._gd.ResourceFactory;

            this._gpuResources = new GPUResourceManager(this._gd, (uint) window.Width, (uint) window.Height);

            this._psoManager = new PSOManager(this._factory);

            this._assetManager = new AssetManager(this._gd, this._gpuResources);

            this._renderer2d     = new GPURenderer2D(this._gd, this._assetManager, this._gpuResources, this._psoManager);
            this._renderer3d     = new GPURenderer3D(this._gd, this._psoManager, this._gpuResources);
            this._imguiRenderer  = new ImguiRenderer(this._gd, this._gpuResources.mainRenderTexture);
            this._mousePicker    = new GPUMousePicker(this._gd);
            this._fullScreenQuad = new FullScreenRenderer(this._gd, this._assetManager, this._psoManager, this._gpuResources);
        }

        public uint selectedObjectID => this._mousePicker.objectID;

        public Vector2 mousePickingPosition = Vector2.Zero;

        public void Update(float deltaTime, InputSnapshot inputSnapshot)
        {
            this._imguiRenderer.Update(deltaTime, inputSnapshot);
        }

        public void Render(IStage stage)
        {
            var render3DTask = Task.Run(() => {
                if (stage.currentScene3D == null) return;
                this._renderer3d.Render(stage.currentScene3D);
            });
            var render2DTask = Task.Run(() => {
                if (stage.currentCanvas2D == null) return;
                this._renderer2d.Render(stage.currentCanvas2D);
            });
            var extraTask = Task.Run(() => {
                this._mousePicker.Update(this._gpuResources.mainRenderTexture, this.mousePickingPosition);
                this._imguiRenderer.Render();
                this._fullScreenQuad.Render();
            });
            Task.WaitAll(render3DTask, render2DTask, extraTask);

            this._gd.WaitForIdle();
            this._renderer3d.Submit();
            this._renderer2d.Submit();
            this._mousePicker.Submit();
            this._imguiRenderer.Submit();
            this._fullScreenQuad.Submit();

            this._gd.SwapBuffers();
        }

        internal void Resize(uint width, uint height)
        {
            this._gd.ResizeMainWindow(width, height);
            this._gd.WaitForIdle();

            this._gpuResources.fullScreenRenderTexture.Resize(width, height);
            this._gpuResources.mainRenderTexture.Resize(width, height);

            this._imguiRenderer.Resize(width, height);
        }

        public void Dispose()
        {
            this._gd.Dispose();
        }
    }
}