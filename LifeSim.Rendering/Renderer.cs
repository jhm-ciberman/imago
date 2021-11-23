using System;
using System.Collections.Generic;
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

        public static SceneStorage? Storage { get; internal set; }

        private readonly Fence _fence;

        private readonly List<Texture> _dirtyTextures = new List<Texture>();

        private readonly CommandList _resourceUpdateCommandList;

        private bool _updatedResources;

        public Renderer(Sdl2Window window, GraphicsBackend graphicsBackend)
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

            this._gd = VeldridStartup.CreateGraphicsDevice(window, options, graphicsBackend);
            Renderer.GraphicsDevice = this._gd;

            this._factory = this._gd.ResourceFactory;

            this.FullScreenRenderTexture = new SwapchainRenderTexture(this._gd.MainSwapchain);
            this.MainRenderTexture = new RenderTexture(this._gd.ResourceFactory, (uint)window.Width, (uint)window.Height);

            this.CanvasRenderer = new CanvasRenderer(this._gd, this.MainRenderTexture);
            this.SceneRenderer = new SceneRenderer(this._gd, this.MainRenderTexture);
            this.ImguiRenderer = new ImguiRenderer(this._gd, this.MainRenderTexture);
            this.MousePicker = new MousePickingRenderer(this._gd, this.MainRenderTexture);
            this.GizmosRenderer = new GizmosRenderer(this._gd, this.MainRenderTexture);
            this.ParticlesRenderer = new ParticlesRenderer(this._gd, this.MainRenderTexture);
            this._fullScreenRenderer = new FullScreenRenderer(this._gd, this.MainRenderTexture, this.FullScreenRenderTexture);

            this._fence = this._factory.CreateFence(false);
            this._resourceUpdateCommandList = this._factory.CreateCommandList();
        }

        public void Render()
        {
            this.UpdateDirtyResources();
            this.ImguiRenderer.Render();
            this._fullScreenRenderer.Render();


            this.WaitForGPU();

            if (this._updatedResources)
            {
                this._updatedResources = false;
                this._gd.SubmitCommands(this._resourceUpdateCommandList);
            }

            this.SceneRenderer.Submit();
            this.ParticlesRenderer.Submit();
            this.GizmosRenderer.Submit();
            this.CanvasRenderer.Submit();
            this.MousePicker.Submit();
            this.ImguiRenderer.Submit();
            this._fullScreenRenderer.Submit(this._fence);
            this._gd.SwapBuffers();
        }

        internal void OnTextureDirty(Texture texture)
        {
            this._dirtyTextures.Add(texture);
        }



        protected void UpdateDirtyResources()
        {
            if (this._dirtyTextures.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Updating {this._dirtyTextures.Count} dirty textures");
                this._resourceUpdateCommandList.Begin();
                foreach (var resource in this._dirtyTextures)
                {
                    resource.Update(this._gd, this._resourceUpdateCommandList);
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