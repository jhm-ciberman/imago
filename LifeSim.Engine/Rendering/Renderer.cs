using System;
using System.Collections.Generic;
using System.Numerics;
using FontStashSharp.Interfaces;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering;

public partial class Renderer : ITexture2DManager, IDisposable
{
    public static Renderer Instance { get; private set; } = null!;

    public IPipelineProvider ForwardPass => this._forwardPass;
    public IPipelineProvider ShadowMapPass => this._shadowPass;
    public SceneStorage Storage { get; }
    public RenderTexture MainRenderTexture { get; }
    public GraphicsDevice GraphicsDevice { get; }
    public GraphicsBackend BackendType => this.GraphicsDevice.BackendType;

    private readonly SwapchainRenderTexture _fullScreenRenderTexture;

    private readonly ResourceFactory _factory;
    private readonly FullScreenPass _fullScreenPass;
    private readonly GizmosPass _gizmosPass;
    private readonly Fence _fence;
    private readonly List<Texture> _dirtyTextures = new List<Texture>();
    private readonly List<MaterialBase> _dirtyMaterials = new List<MaterialBase>();
    private readonly List<Renderable> _dirtyRenderables = new List<Renderable>();
    private readonly ForwardPass _forwardPass;
    private readonly ShadowPass _shadowPass;
    private readonly SpritesPass _spritesPass;
    private readonly ImGuiPass _imGuiPass;
    private readonly MousePickingPass _mousePickerPass;
    private readonly ParticlesPass _particlesPass;
    private readonly SkyDomePass _skyDomePass;
    private readonly CommandList _commandList;
    private readonly List<CommandListJob> _jobs;
    private readonly DisposeCollector _disposeCollector;

    private readonly Dictionary<ResourceLayoutDescription, ResourceLayout> _resourceLayoutCache = new Dictionary<ResourceLayoutDescription, ResourceLayout>();

    public Renderer(Sdl2Window window, GraphicsBackend? graphicsBackend = null)
    {
        if (Instance != null)
        {
            throw new InvalidOperationException("Only one instance of Renderer can be created.");
        }

        Instance = this;

        GraphicsDeviceOptions options = new GraphicsDeviceOptions(
            debug: false,
            swapchainDepthFormat: null, //PixelFormat.R16_UNorm,
            syncToVerticalBlank: true,
            resourceBindingModel: ResourceBindingModel.Improved,
            preferDepthRangeZeroToOne: true,
            preferStandardClipSpaceYDirection: true,
            swapchainSrgbFormat: false
        );

        var gd = VeldridStartup.CreateGraphicsDevice(window, options, graphicsBackend ?? VeldridStartup.GetPlatformDefaultBackend());
        this.GraphicsDevice = gd;

        this._disposeCollector = new DisposeCollector();
        this._factory = this.GraphicsDevice.ResourceFactory;

        this._fullScreenRenderTexture = new SwapchainRenderTexture();
        this.MainRenderTexture = new RenderTexture((uint)window.Width, (uint)window.Height);

        this.Storage = new SceneStorage(gd);
        Renderable.PipelineDirty += this.OnRenderablePipelineDirty;
        MaterialBase.MaterialResourceSetDirty += this.OnMaterialResourceSetDirty;
        Texture.TextureDirty += this.OnTextureDirty;


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

        this._jobs = new List<CommandListJob>
        {
            new CommandListJob("Shadows", this._factory,
                this._shadowPass),
            new CommandListJob("Forward", this._factory,
                this._forwardPass),
            new CommandListJob("Extra", this._factory,
                this._skyDomePass,
                this._mousePickerPass,
                this._particlesPass,
                this._gizmosPass,
                this._spritesPass,
                this._imGuiPass),
            new CommandListJob("Present", this._factory,
                this._fullScreenPass),
        };
    }

    public void DisposeWhenIdle(IDisposable disposable)
    {
        lock (this._disposeCollector)
        {
            this._disposeCollector.Add(disposable);
        }
    }

    public void DisposeWhenIdle(IDisposable[] disposables)
    {
        foreach (var disposable in disposables)
        {
            this.DisposeWhenIdle(disposable);
        }
    }

    public Renderable MakeRenderable(int instanceDataBlockSize)
    {
        return new Renderable(this.Storage, instanceDataBlockSize);
    }

    protected void UpdateDirtyMaterials()
    {
        lock (this._dirtyMaterials)
        {
            if (this._dirtyMaterials.Count > 0)
            {
                foreach (var material in this._dirtyMaterials)
                {
                    material.Update(this._factory);
                }
                this._dirtyMaterials.Clear();
            }
        }
    }

    private void UpdateDirtyTextures()
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

    private void UpdateDirtyRenderables()
    {
        lock (this._dirtyRenderables)
        {
            if (this._dirtyRenderables.Count > 0)
            {
                foreach (var renderable in this._dirtyRenderables)
                {
                    renderable.Update(this);
                }
                this._dirtyRenderables.Clear();
            }
        }
    }

    public void SetMousePickingPosition(Vector2 position)
    {
        this._mousePickerPass.SetMousePosition(position);
    }

    public void UpdateImGui(float deltaTime, InputSnapshot inputSnapshot)
    {
        this._imGuiPass.Update(deltaTime, inputSnapshot);
    }

    public void Render(Scene scene)
    {
        scene.OnBeforeRender();
        scene.RenderImGui();
        scene.UpdateSceneDirtyTransforms();

        this._commandList.Begin();
        this.UpdateDirtyMaterials();
        this.UpdateDirtyTextures();
        this.UpdateDirtyRenderables();
        this.Storage.UpdateBuffers(this._commandList);

        this._commandList.SetFramebuffer(this.MainRenderTexture.Framebuffer);

        if (scene.BackgroundColor != null)
        {
            ColorF col = scene.BackgroundColor.Value;
            this._commandList.ClearColorTarget(0, new RgbaFloat(col.R, col.G, col.B, col.A));
        }
        this._commandList.End();

        for (int i = 0; i < this._jobs.Count; i++)
        {
            this._jobs[i].Execute(scene);
        }


        this.GraphicsDevice.WaitForIdle();
        this._fence.Reset();
        this.GraphicsDevice.SubmitCommands(this._commandList, this._fence);

        for (int i = 0; i < this._jobs.Count; i++)
        {
            this._jobs[i].SubmitCommands(this.GraphicsDevice);
        }

        this._disposeCollector.DisposeAll();
        this.GraphicsDevice.SwapBuffers();
    }

    public IntPtr GetOrCreateImGuiBinding(Texture texture)
    {
        return this._imGuiPass.GetOrCreateBinding(texture);
    }

    private void OnTextureDirty(Texture texture)
    {
        lock (this._dirtyTextures)
        {
            this._dirtyTextures.Add(texture);
        }
    }

    private void OnMaterialResourceSetDirty(MaterialBase material)
    {
        lock (this._dirtyMaterials)
        {
            this._dirtyMaterials.Add(material);
        }
    }

    private void OnRenderablePipelineDirty(Renderable renderable)
    {
        lock (this._dirtyRenderables)
        {
            this._dirtyRenderables.Add(renderable);
        }
    }

    public ResourceLayout GetResourceLayout(ResourceLayoutDescription description)
    {
        lock (this._resourceLayoutCache)
        {
            if (!this._resourceLayoutCache.TryGetValue(description, out ResourceLayout? layout))
            {
                layout = this._factory.CreateResourceLayout(description);
                this._resourceLayoutCache.Add(description, layout);
            }
            return layout;
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

        foreach (var job in this._jobs)
        {
            job.Dispose();
        }
    }

    public RenderNode3D? SelectedRenderNode => this._mousePickerPass.SelectedRenderNode;

    public uint RegisterPickable(RenderNode3D renderNode)
    {
        return this._mousePickerPass.RegisterPickable(renderNode);
    }

    public void UnregisterPickable(uint pickingId)
    {
        this._mousePickerPass.UnregisterPickable(pickingId);
    }

    public void AddImmediateRenderNode(ImmediateRenderNode3D node)
    {
        this._forwardPass.AddImmediateRenderNode(node);
    }

    public void RemoveImmediateRenderNode(ImmediateRenderNode3D node)
    {
        this._forwardPass.RemoveImmediateRenderNode(node);
    }

    object ITexture2DManager.CreateTexture(int width, int height)
    {
        return new DirectTexture((uint)width, (uint)height);
    }

    void ITexture2DManager.SetTextureData(object texture, System.Drawing.Rectangle bounds, byte[] data)
    {
        ((DirectTexture)texture).Update(data, bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    public delegate void GlobalRenderSettingsChangedEventHandler(Renderer renderer);

    public event GlobalRenderSettingsChangedEventHandler? GlobalRenderSettingsChanged;

    private bool _forceWireframe = false;
    public bool ForceWireframe
    {
        get => _forceWireframe;
        set
        {
            if (_forceWireframe == value) return;
            _forceWireframe = value;
            GlobalRenderSettingsChanged?.Invoke(this);
        }
    }

    private bool _enableFog = true;

    public bool EnableFog
    {
        get => _enableFog;
        set
        {
            if (_enableFog == value) return;
            _enableFog = value;
            GlobalRenderSettingsChanged?.Invoke(this);
        }
    }
}