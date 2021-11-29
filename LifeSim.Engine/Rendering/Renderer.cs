using System;
using System.Collections.Generic;
using System.Numerics;
using FontStashSharp.Interfaces;
using LifeSim.Engine.SceneGraph;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace LifeSim.Engine.Rendering
{
    public class Renderer : ITexture2DManager, IDisposable
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
        private readonly FullScreenPass _fullScreenPass;
        private readonly GizmosPass _gizmosPass;



        public GraphicsBackend BackendType => this.GraphicsDevice.BackendType;



        private readonly Fence _fence;

        private readonly List<Texture> _dirtyTextures = new List<Texture>();

        private readonly List<Material> _dirtyMaterials = new List<Material>();

        private readonly ForwardPass _forwardPass;
        private readonly ShadowmapPass _shadowmapPass;
        private readonly SpritesPass _spritesPass;
        private readonly ImGuiPass _imGuiPass;
        private readonly MousePickingPass _mousePickerPass;
        private readonly ParticlesPass _particlesPass;

        private readonly SpriteBatcher _spriteBatcher;

        public IPipelineProvider ForwardPass => this._forwardPass;
        public IPipelineProvider ShadowMapPass => this._shadowmapPass;

        internal SceneStorage Storage { get; }
        public uint MousePickerObjectID => this._mousePickerPass.ObjectID;

        private readonly CommandList _commandList;

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
            this.MainRenderTexture = new RenderTexture(gd, (uint)window.Width, (uint)window.Height);

            this.Storage = new SceneStorage(gd);

            this._imGuiPass = new ImGuiPass(gd, this.MainRenderTexture);
            this._mousePickerPass = new MousePickingPass(gd, this.MainRenderTexture);
            this._gizmosPass = new GizmosPass(gd, this.MainRenderTexture);
            this._particlesPass = new ParticlesPass(gd, this.MainRenderTexture);
            this._fullScreenPass = new FullScreenPass(gd, this.MainRenderTexture, this.FullScreenRenderTexture);

            this._commandList = this._factory.CreateCommandList();
            this._shadowmapPass = new ShadowmapPass(gd, this.Storage);
            this._forwardPass = new ForwardPass(gd, this.Storage, this.MainRenderTexture, this._shadowmapPass.ShadowmapTexture);
            this._spritesPass = new SpritesPass(gd, this.MainRenderTexture);

            this._spriteBatcher = new SpriteBatcher(gd, this._spritesPass.Shader);

            this._fence = this._factory.CreateFence(false);
        }

        public void SetMousePickingPosition(Vector2 position)
        {
            this._mousePickerPass.SetMousePosition(position);
        }

        public void UpdateImGui(float deltaTime, InputSnapshot inputSnapshot)
        {
            this._imGuiPass.Update(deltaTime, inputSnapshot);
        }

        public void BeginRender()
        {
            this._commandList.Begin();

            this.UpdateDirtyMaterials();
            this.UpdateDirtyTextures();
        }

        protected void UpdateDirtyMaterials()
        {
            if (this._dirtyTextures.Count > 0)
            {
                foreach (var material in this._dirtyMaterials)
                {
                    material.Update();
                }
                this._dirtyMaterials.Clear();
            }
        }

        protected void UpdateDirtyTextures()
        {
            if (this._dirtyTextures.Count > 0)
            {
                foreach (var resource in this._dirtyTextures)
                {
                    resource.Update(this.GraphicsDevice, this._commandList);
                }
                this._dirtyTextures.Clear();
            }
        }

        public void RenderCanvas(Viewport viewport, IReadOnlyList<ICanvasItem> items)
        {
            Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, -10f, 100f);

            this._spriteBatcher.BeginBatch();
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Render(this._spriteBatcher);
            }


            this._spritesPass.BeginPass(this._commandList, ref projection);
            this._spritesPass.SubmitBatches(this._commandList, this._spriteBatcher.IndexBuffer, this._spriteBatcher.Batches);
        }

        public void RenderScene(IReadOnlyList<Renderable> renderables, DirectionalLight mainLight, ColorF ambientColor, ColorF clearColor, ICamera camera)
        {
            this.Storage.UpdateBuffers(this._commandList);
            this._shadowmapPass.Render(this._commandList, renderables, camera, mainLight);
            this._forwardPass.Render(this._commandList, renderables, mainLight, ambientColor, clearColor, camera);
        }

        public void RenderGizmos(IReadOnlyList<DebugLine> lines, ICamera camera3D)
        {
            this._gizmosPass.Render(this._commandList, lines, camera3D);
        }

        public void RenderParticles(SwapPopList<Particle> particles, Texture texture, ICamera camera)
        {
            this._particlesPass.Render(this._commandList, particles, texture, camera);
        }

        public void EndRender()
        {
            this._mousePickerPass.Render(this._commandList);
            this._imGuiPass.Render(this._commandList);
            this._fullScreenPass.Render(this._commandList);
            this._commandList.End();

            if (!this._fence.Signaled)
            { // If we are GPU bound, then maybe it's a good moment to do a GC :)
                this._fence.Reset();
                GC.Collect(0, GCCollectionMode.Optimized);
            }
            this.GraphicsDevice.WaitForIdle();

            this.GraphicsDevice.SubmitCommands(this._commandList, this._fence);
            this.GraphicsDevice.SwapBuffers();
        }

        public IntPtr GetOrCreateImGuiBinding(Texture texture)
        {
            return this._imGuiPass.GetOrCreateBinding(texture);
        }

        internal void OnTextureDirty(Texture texture)
        {
            this._dirtyTextures.Add(texture);
        }

        internal void OnMaterialDirty(Material material)
        {
            this._dirtyMaterials.Add(material);
        }

        public void Resize(uint width, uint height, uint viewportWidth, uint viewportHeight)
        {
            this.GraphicsDevice.ResizeMainWindow(width, height);
            this.GraphicsDevice.WaitForIdle();
            this.FullScreenRenderTexture.Resize(width, height);
            this.MainRenderTexture.Resize(viewportWidth, viewportHeight);
            this._imGuiPass.Resize(viewportWidth, viewportHeight);
        }

        public void Dispose()
        {
            this.GraphicsDevice.WaitForIdle();
            this.FullScreenRenderTexture.Dispose();
            this.MainRenderTexture.Dispose();
            this._imGuiPass.Dispose();
            this._mousePickerPass.Dispose();
            this._gizmosPass.Dispose();
            this._particlesPass.Dispose();
            this._spritesPass.Dispose();
            this._fullScreenPass.Dispose();
            this._commandList.Dispose();
            this._fence.Dispose();
            this.GraphicsDevice.Dispose();
        }

        object ITexture2DManager.CreateTexture(int width, int height)
        {
            return new Texture(width, height);
        }

        void ITexture2DManager.SetTextureData(object texture, System.Drawing.Rectangle bounds, byte[] data)
        {
            var t = (Texture) texture;
            t.SetDataFromBytes(bounds.X, bounds.Y, bounds.Width, bounds.Height, data);
        }
    }
}