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
        private RenderTexture _mainRenderTexture;

        private FullScreenRenderer _fullScreenQuad;

        public GraphicsBackend backendType => this._graphicsDevice.BackendType;

        private SceneContext _sceneContext;
        private MaterialManager _materialManager;

        private Veldrid.Texture _pixelTexture;
        private CommandList _commandList;
    
        private Fence _fence;

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

            System.Console.WriteLine(this._graphicsDevice.GetPixelFormatSupport(
                PixelFormat.R32_UInt, 
                TextureType.Texture2D, 
                TextureUsage.RenderTarget | TextureUsage.Sampled
            ));

            this._pixelTexture = this._factory.CreateTexture(new TextureDescription(
                width: 1, height: 1, depth: 1, mipLevels: 1, arrayLayers: 1, 
                PixelFormat.R32_UInt, TextureUsage.Staging, TextureType.Texture2D
            ));

            this._commandList = this._factory.CreateCommandList();

            this._fence = this._factory.CreateFence(false);

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


            var mousePos = Input.MousePosition;
            if (mousePos.Y < this._mainRenderTexture.pickingTexture.Height) {
                uint x = (uint) mousePos.X;
                uint y;
                if (this._graphicsDevice.IsUvOriginTopLeft) {
                    y = (uint) (mousePos.Y);
                } else {
                    y = (uint) (this._mainRenderTexture.pickingTexture.Height - 1 - mousePos.Y);
                }
                this._commandList.Begin();
                this._commandList.CopyTexture(
                    source: this._mainRenderTexture.pickingTexture, 
                    srcX: x, srcY: y, srcZ: 0, srcMipLevel: 0, srcBaseArrayLayer: 0, 
                    destination: this._pixelTexture, 
                    dstX: 0, dstY: 0, dstZ: 0, dstMipLevel: 0, dstBaseArrayLayer: 0, 
                    width: 1, height: 1, depth: 1, layerCount: 1
                );
                this._commandList.End();
                this._graphicsDevice.SubmitCommands(this._commandList, this._fence);

                this._graphicsDevice.WaitForFence(this._fence);
                this._fence.Reset();
            }


            var mappedResource = this._graphicsDevice.Map<uint>(this._pixelTexture, MapMode.Read);
            var objID = mappedResource[0, 0];
            if (objID != 0) {
                System.Console.WriteLine(objID);
            }

            this._fullScreenQuad.Render();

            this._graphicsDevice.Unmap(this._pixelTexture);
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