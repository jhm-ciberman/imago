using System.Text;
using System.Threading.Tasks;
using LifeSim.SceneGraph;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.StartupUtilities;

namespace LifeSim.Rendering
{
    public class GPURenderer : System.IDisposable
    {
        private GraphicsDevice _gd;
        private ResourceFactory _factory;
        
        private GPURenderer2D _renderer2d;
        private GPURenderer3D _renderer3d;

        private FullScreenRenderer _fullScreenQuad;

        public GraphicsBackend backendType => this._gd.BackendType;

        private GPUMousePicker _mousePicker;

        private GPUResources _gpuResources;

        private PSOManager _psoManager;

        public GPURenderer(Window window, GraphicsBackend graphicsBackend)
        {
            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: true,
                resourceBindingModel: ResourceBindingModel.Default,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true
            );

            this._gd = VeldridStartup.CreateGraphicsDevice(window.nativeWindow, options, graphicsBackend);
            this._factory = this._gd.ResourceFactory;

            this._gpuResources = new GPUResources(this._gd, window.width, window.height);

            this._psoManager = new PSOManager(this._factory);

            this._renderer2d     = new GPURenderer2D(this._gd, this, this._gpuResources, this._psoManager);
            this._renderer3d     = new GPURenderer3D(this._gd, this._psoManager, this._gpuResources);
            this._mousePicker    = new GPUMousePicker(this._gd);
            this._fullScreenQuad = new FullScreenRenderer(this._gd, this, this._psoManager, this._gpuResources);
        }

        public GPUTexture MakeTexture(string path)
        {
            ImageSharpTexture texture = new ImageSharpTexture(path, true);
            return new GPUTexture(this._gd, texture);
        }

        public GPUTexture MakeTexture(Image<Rgba32> image)
        {
            ImageSharpTexture texture = new ImageSharpTexture(image, false);
            return new GPUTexture(this._gd, texture);
        }

        public GLTF.GLTFLoader LoadGLTF(string path, SurfaceMaterial defaultMaterial)
        {
            return new GLTF.GLTFLoader(this, defaultMaterial, path);
        }

        public GPUMesh MakeMesh(MeshData meshData)
        {
            return new GPUMesh(this._gd, meshData);
        }

        public SurfaceMaterial MakeSurfaceMaterial(GPUTexture texture)
        {
            return new SurfaceMaterial(this._gpuResources, texture);
        }

        public SpriteMaterial MakeSpritesMaterial(Veldrid.Texture texture)
        {
            return new SpriteMaterial(this._gpuResources, texture);
        }

        public FullScreenMaterial MakeFullScreenMaterial(Veldrid.Texture texture)
        {
            return new FullScreenMaterial(this._gpuResources, texture);
        }

        public uint selectedObjectID => this._mousePicker.objectID;

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
                this._mousePicker.Update(this._gpuResources.mainRenderTexture);
                this._fullScreenQuad.Render();
            });
            Task.WaitAll(render3DTask, render2DTask, extraTask);

            this._gd.WaitForIdle();
            this._renderer3d.Submit();
            this._renderer2d.Submit();
            this._mousePicker.Submit();
            this._fullScreenQuad.Submit();

            this._gd.SwapBuffers();
        }

        internal void Resize(uint width, uint height)
        {
            this._gd.ResizeMainWindow(width, height);
            this._gd.WaitForIdle();

            this._gpuResources.fullScreenRenderTexture.Resize(width, height);
            this._gpuResources.mainRenderTexture.Resize(width, height);
        }

        public void Dispose()
        {
            this._gd.Dispose();
        }
    }
}