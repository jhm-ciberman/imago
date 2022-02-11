using System;
using System.Collections.Generic;
using System.Numerics;
using FontStashSharp.Interfaces;
using LifeSim.Engine.Resources;
using LifeSim.Engine.SceneGraph;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace LifeSim.Engine.Rendering;

public class Renderer : ITexture2DManager, IDisposable
{
    private readonly SwapchainRenderTexture _fullScreenRenderTexture;
    public RenderTexture MainRenderTexture { get; }

    public GraphicsDevice GraphicsDevice { get; }
    private readonly ResourceFactory _factory;
    private readonly FullScreenPass _fullScreenPass;
    private readonly GizmosPass _gizmosPass;

    public GraphicsBackend BackendType => this.GraphicsDevice.BackendType;

    private readonly Fence _fence;

    private readonly List<Texture> _dirtyTextures = new List<Texture>();

    private readonly List<Material> _dirtyMaterials = new List<Material>();

    private readonly ForwardPass _forwardPass;
    private readonly ShadowPass _shadowPass;
    private readonly SpritesPass _spritesPass;
    private readonly ImGuiPass _imGuiPass;
    private readonly MousePickingPass _mousePickerPass;
    private readonly ParticlesPass _particlesPass;
    private readonly SkyDomePass _skyDomePass;
    public IPipelineProvider ForwardPass => this._forwardPass;
    public IPipelineProvider ShadowMapPass => this._shadowPass;


    public SceneStorage Storage { get; }
    public uint MousePickerObjectID => this._mousePickerPass.ObjectID;

    private readonly CommandList _commandList;

    public ShadowMapConfig ShadowMapConfig => this._shadowPass.Config;

    public ITexture ShadowMapTexture => this._shadowPass.ShadowmapTexture;

    public RendererResourceFactory Factory { get; }

    private readonly List<IRenderingPass> _passes = new List<IRenderingPass>();

    public Renderer(Sdl2Window window, GraphicsBackend? graphicsBackend = null)
    {
        GraphicsDeviceOptions options = new GraphicsDeviceOptions(
            debug: false,
            swapchainDepthFormat: null, //PixelFormat.R16_UNorm,
            syncToVerticalBlank: true,
            resourceBindingModel: ResourceBindingModel.Default,
            preferDepthRangeZeroToOne: true,
            preferStandardClipSpaceYDirection: true,
            swapchainSrgbFormat: false
        );

        this.Factory = new RendererResourceFactory(this);

        var gd = VeldridStartup.CreateGraphicsDevice(window, options, graphicsBackend ?? VeldridStartup.GetPlatformDefaultBackend());
        this.GraphicsDevice = gd;

        this._factory = this.GraphicsDevice.ResourceFactory;

        this._fullScreenRenderTexture = new SwapchainRenderTexture(this, this.GraphicsDevice.MainSwapchain);
        this.MainRenderTexture = new RenderTexture(this, (uint)window.Width, (uint)window.Height);

        this.Storage = new SceneStorage(gd);

        this._imGuiPass = new ImGuiPass(this, this.MainRenderTexture);
        this._mousePickerPass = new MousePickingPass(this, this.MainRenderTexture);
        this._gizmosPass = new GizmosPass(this, this.MainRenderTexture);
        this._particlesPass = new ParticlesPass(this, this.MainRenderTexture);
        this._shadowPass = new ShadowPass(this, this.Storage);
        this._forwardPass = new ForwardPass(this, this.Storage, this.MainRenderTexture, this._shadowPass);
        this._spritesPass = new SpritesPass(this, this.MainRenderTexture);
        this._skyDomePass = new SkyDomePass(this, this.MainRenderTexture);

        this._fullScreenPass = new FullScreenPass(this, this.MainRenderTexture, this._fullScreenRenderTexture);
        this._commandList = this._factory.CreateCommandList();

        this._fence = this._factory.CreateFence(false);

        this._passes.AddRange(new IRenderingPass[]
        {
            this._shadowPass,
            this._forwardPass,
            this._gizmosPass,
            this._skyDomePass,
            this._particlesPass,
            this._spritesPass,
            this._mousePickerPass,
            this._imGuiPass,
            this._fullScreenPass
        });
    }

    protected void UpdateDirtyMaterials()
    {
        lock (this._dirtyMaterials)
        {
            if (this._dirtyMaterials.Count > 0)
            {
                foreach (var material in this._dirtyMaterials)
                {
                    material.Update();
                }
                this._dirtyMaterials.Clear();
            }
        }
    }

    public void UpdateDirtyTextures()
    {
        lock (this._dirtyTextures)
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
    }

    public void Render(Scene scene, float deltaTime, InputSnapshot inputSnapshot)
    {
        this._mousePickerPass.SetMousePosition(inputSnapshot.MousePosition);
        this._imGuiPass.Update(deltaTime, inputSnapshot);

        scene.OnBeforeRender(this);
        scene.RenderImGui();

        this._commandList.Begin();

        this.UpdateDirtyMaterials();
        this.UpdateDirtyTextures();
        this.Storage.UpdateBuffers(this._commandList);

        this._commandList.SetFramebuffer(this.MainRenderTexture.Framebuffer);

        for (int i = 0; i < this._passes.Count; i++)
        {
            this._passes[i].Render(this._commandList, scene);
        }

        this._commandList.End();

        if (!this._fence.Signaled)
        {
            // If we are GPU bound, then maybe it's a good moment to do a GC :)
            Console.WriteLine("Performing GC");
            GC.Collect(0, GCCollectionMode.Optimized);
        }
        this.GraphicsDevice.WaitForIdle();
        this._fence.Reset();
        this.GraphicsDevice.SubmitCommands(this._commandList, this._fence);
        this.GraphicsDevice.SwapBuffers();
    }

    public IntPtr GetOrCreateImGuiBinding(Texture texture)
    {
        return this._imGuiPass.GetOrCreateBinding(texture);
    }

    internal void OnTextureDirty(Texture texture)
    {
        lock (this._dirtyTextures)
        {
            this._dirtyTextures.Add(texture);
        }
    }

    internal void OnMaterialDirty(Material material)
    {
        lock (this._dirtyMaterials)
        {
            this._dirtyMaterials.Add(material);
        }
    }

    public void Resize(uint width, uint height, uint viewportWidth, uint viewportHeight)
    {
        this.GraphicsDevice.ResizeMainWindow(width, height);
        this.GraphicsDevice.WaitForIdle();
        this._fullScreenRenderTexture.Resize(width, height);
        this.MainRenderTexture.Resize(viewportWidth, viewportHeight);
        this._imGuiPass.Resize(viewportWidth, viewportHeight);
    }

    public void Dispose()
    {
        this.GraphicsDevice.WaitForIdle();
        this._fullScreenRenderTexture.Dispose();
        this.MainRenderTexture.Dispose();
        this._commandList.Dispose();
        this._fence.Dispose();
        this.GraphicsDevice.Dispose();

        foreach (var pass in this._passes)
        {
            pass.Dispose();
        }
    }

    object ITexture2DManager.CreateTexture(int width, int height)
    {
        return new DirectTexture(this, (uint)width, (uint)height);
    }

    void ITexture2DManager.SetTextureData(object texture, System.Drawing.Rectangle bounds, byte[] data)
    {
        ((DirectTexture)texture).Update(data, bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }
}