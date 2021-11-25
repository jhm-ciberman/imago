using System;
using System.Collections.Generic;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace LifeSim.Engine.Rendering
{
    public class Renderer : IDisposable
    {
        // This is the only global variable! I swear!! 
        // Please don't point your finger at me with that face (?)
        private static Renderer? _instance = null;
        public static Renderer Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException("Renderer has not been initialized!");
                }
                return _instance;
            }
        }

        public readonly IRenderTexture FullScreenRenderTexture;
        public readonly RenderTexture MainRenderTexture;

        public GraphicsDevice GraphicsDevice { get; }
        private readonly ResourceFactory _factory;
        private readonly FullScreenRenderer _fullScreenRenderer;

        public GizmosRenderer GizmosRenderer { get; }

        public ParticlesRenderer ParticlesRenderer { get; }

        public ImguiRenderer ImguiRenderer { get; }

        public CanvasRenderer CanvasRenderer { get; }

        public MousePickingRenderer MousePicker { get; }
        public GraphicsBackend BackendType => this.GraphicsDevice.BackendType;

        private readonly Fence _fence;

        private readonly List<Texture> _dirtyTextures = new List<Texture>();

        private readonly List<Material> _dirtyMaterials = new List<Material>();

        private readonly CommandList _resourceUpdateCommandList;

        private readonly ForwardPass _forwardPass;
        private readonly ShadowmapPass _shadowmapPass;

        public IPipelineProvider ForwardPass => this._forwardPass;
        public IPipelineProvider ShadowMapPass => this._shadowmapPass;

        internal SceneStorage Storage { get; }

        private readonly CommandList _commandList;

        private bool _hasCommandsToSubmit;

        private bool _updatedResources;

        public Renderer(Sdl2Window window, GraphicsBackend? graphicsBackend = null)
        {
            if (_instance != null)
            {
                throw new InvalidOperationException("Renderer has already been initialized!");
            }
            _instance = this;

            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: false,
                resourceBindingModel: ResourceBindingModel.Default,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true,
                swapchainSrgbFormat: false
            );

            var gd = VeldridStartup.CreateGraphicsDevice(window, options, graphicsBackend ?? VeldridStartup.GetPlatformDefaultBackend());
            this.GraphicsDevice = gd;

            this._factory = this.GraphicsDevice.ResourceFactory;

            this.FullScreenRenderTexture = new SwapchainRenderTexture(this.GraphicsDevice.MainSwapchain);
            this.MainRenderTexture = new RenderTexture(gd.ResourceFactory, (uint)window.Width, (uint)window.Height);

            this.Storage = new SceneStorage(gd);

            this.CanvasRenderer = new CanvasRenderer(gd, this.MainRenderTexture);
            this.ImguiRenderer = new ImguiRenderer(gd, this.MainRenderTexture);
            this.MousePicker = new MousePickingRenderer(gd, this.MainRenderTexture);
            this.GizmosRenderer = new GizmosRenderer(gd, this.MainRenderTexture);
            this.ParticlesRenderer = new ParticlesRenderer(gd, this.MainRenderTexture);
            this._fullScreenRenderer = new FullScreenRenderer(gd, this.MainRenderTexture, this.FullScreenRenderTexture);

            this._commandList = this._factory.CreateCommandList();
            this._shadowmapPass = new ShadowmapPass(gd, this.Storage);
            this._forwardPass = new ForwardPass(gd, this.Storage, this.MainRenderTexture, this._shadowmapPass.ShadowmapTexture);

            this._fence = this._factory.CreateFence(false);
            this._resourceUpdateCommandList = this._factory.CreateCommandList();
        }

        public void BeginRender()
        {
            this.UpdateDirtyResources();
        }

        public void Render(IReadOnlyList<Renderable> renderables, DirectionalLight mainLight, ColorF ambientColor, ColorF clearColor, ICamera camera)
        {
            this._commandList.Begin();
            this.Storage.UpdateBuffers(this._commandList);
            this._shadowmapPass.Render(this._commandList, renderables, camera, mainLight);
            this._forwardPass.Render(this._commandList, renderables, mainLight, ambientColor, clearColor, camera);
            this._commandList.End();

            this._hasCommandsToSubmit = true;
        }

        public void Render()
        {
            this.ImguiRenderer.Render();
            this._fullScreenRenderer.Render();

            this.WaitForGPU();

            if (this._updatedResources)
            {
                this._updatedResources = false;
                this.GraphicsDevice.SubmitCommands(this._resourceUpdateCommandList);
            }

            if (this._hasCommandsToSubmit)
            {
                this.GraphicsDevice.SubmitCommands(this._commandList);
                this._hasCommandsToSubmit = false;
            }

            this.ParticlesRenderer.Submit();
            this.GizmosRenderer.Submit();
            this.CanvasRenderer.Submit();
            this.MousePicker.Submit();
            this.ImguiRenderer.Submit();
            this._fullScreenRenderer.Submit(this._fence);
            this.GraphicsDevice.SwapBuffers();
        }

        internal void OnTextureDirty(Texture texture)
        {
            this._dirtyTextures.Add(texture);
        }

        internal void OnMaterialDirty(Material material)
        {
            this._dirtyMaterials.Add(material);
        }

        protected void UpdateDirtyResources()
        {
            if (this._dirtyTextures.Count == 0 && this._dirtyMaterials.Count == 0)
            {
                return;
            }

            foreach (var material in this._dirtyMaterials)
            {
                material.Update();
            }
            this._dirtyMaterials.Clear();

            if (this._dirtyTextures.Count > 0)
            {
                this._resourceUpdateCommandList.Begin();
                foreach (var resource in this._dirtyTextures)
                {
                    resource.Update(this.GraphicsDevice, this._resourceUpdateCommandList);
                }
                this._resourceUpdateCommandList.End();
                this._dirtyTextures.Clear();
                this._updatedResources = true;
            }
        }

        public void WaitForGPU()
        {
            if (!this._fence.Signaled)
            { // If we are GPU bound, then maybe it's a good moment to do a GC :)
                this._fence.Reset();
                GC.Collect(0, GCCollectionMode.Optimized);
            }
            this.GraphicsDevice.WaitForIdle();
        }

        public void Resize(uint width, uint height, uint viewportWidth, uint viewportHeight)
        {
            this.GraphicsDevice.ResizeMainWindow(width, height);
            this.GraphicsDevice.WaitForIdle();
            this.FullScreenRenderTexture.Resize(width, height);
            this.MainRenderTexture.Resize(viewportWidth, viewportHeight);
            this.ImguiRenderer.Resize(viewportWidth, viewportHeight);
        }

        public void Dispose()
        {
            this.FullScreenRenderTexture.Dispose();
            this.MainRenderTexture.Dispose();
            this.CanvasRenderer.Dispose();
            this.ImguiRenderer.Dispose();
            this.MousePicker.Dispose();
            this.GizmosRenderer.Dispose();
            this._fullScreenRenderer.Dispose();
            this._commandList.Dispose();
            this._fence.Dispose();
            this.GraphicsDevice.Dispose();
        }
    }
}