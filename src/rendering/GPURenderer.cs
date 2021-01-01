using System.Text;
using System.Threading.Tasks;
using LifeSim.SceneGraph;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.StartupUtilities;

namespace LifeSim.Rendering
{
    public class GPURenderer : System.IDisposable
    {
        private GraphicsDevice _graphicsDevice;
        public GraphicsDevice graphicsDevice => this._graphicsDevice;

        private ResourceFactory _factory;

        private GPURenderer2D _renderer2d;
        private GPURenderer3D _renderer3d;

        private IRenderTexture _fullScreenRenderTexture;
        private IRenderTexture _mainRenderTexture;

        private FullScreenRenderer _fullScreenQuad;

        public GraphicsBackend backendType => this._graphicsDevice.BackendType;

        private SceneContext _sceneContext;
        private MaterialManager _materialManager;

        public GPURenderer(Window window, GraphicsBackend graphicsBackend)
        {
            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: false,
                resourceBindingModel: ResourceBindingModel.Default,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true
            );

            this._graphicsDevice = VeldridStartup.CreateGraphicsDevice(window.nativeWindow, options, graphicsBackend);
            this._factory = this._graphicsDevice.ResourceFactory;

            this._fullScreenRenderTexture = new SwapchainRenderTexture(this._graphicsDevice.MainSwapchain);
            this._mainRenderTexture = new RenderTexture(this._factory, window.width, window.height);

            this._sceneContext = new SceneContext(this._factory);
            this._materialManager = new MaterialManager(this._graphicsDevice, this._mainRenderTexture, this._fullScreenRenderTexture, this._sceneContext);

            this._renderer2d = new GPURenderer2D(this._graphicsDevice, this._materialManager, this._sceneContext, this._mainRenderTexture);
            this._renderer3d = new GPURenderer3D(this._graphicsDevice, this._sceneContext, this._mainRenderTexture);

            this._fullScreenQuad = new FullScreenRenderer(this._graphicsDevice, this._materialManager, this._mainRenderTexture, this._fullScreenRenderTexture);
        }

        public MaterialManager materialManager => this._materialManager;

        public GPUTexture MakeTexture(string path)
        {
            ImageSharpTexture texture = new ImageSharpTexture(path, true);
            var deviceTexture = texture.CreateDeviceTexture(this._graphicsDevice, this._factory);
            var textureView = this._factory.CreateTextureView(deviceTexture);
            
            return new GPUTexture(deviceTexture, textureView, this._graphicsDevice.PointSampler);
        }

        public GPUMesh MakeMesh(MeshData meshData)
        {
            return new GPUMesh(this._factory, this._graphicsDevice, meshData);
        }

        public void Render(Scene3D scene)
        {
            var render3DTask = Task.Run(() => {
                this._renderer3d.Render(scene);
            });
            var render2DTask = Task.Run(() => {
                this._renderer2d.Render(scene);
            });
            Task.WaitAll(render3DTask, render2DTask);
            this._renderer3d.Submit();
            this._renderer2d.Submit();

            this._fullScreenQuad.Render();

            this._graphicsDevice.SwapBuffers();
        }

        internal void Resize(uint width, uint height)
        {
            this._graphicsDevice.ResizeMainWindow(width, height);
            this._graphicsDevice.WaitForIdle();

            this._fullScreenRenderTexture.Resize(width, height);
            this._mainRenderTexture.Resize(width, height);
        }

        public void Dispose()
        {
            this._graphicsDevice.Dispose();
        }
    }
}