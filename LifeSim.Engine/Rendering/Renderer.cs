using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using FontStashSharp.Interfaces;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;
using LifeSim.Utils;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering;

public partial class Renderer : IDisposable
{
    /// <summary>
    /// Gets the global instance of the renderer.
    /// </summary>
    public static Renderer Instance { get; private set; } = null!;

    /// <summary>
    /// Event that is raised at the start of the rendering of each frame.
    /// </summary>
    public event EventHandler? RenderStarted;

    /// <summary>
    /// Event that is raised at the end of the rendering of each frame.
    /// </summary>
    public event EventHandler? RenderEnded;

    /// <summary>
    /// Get the forward pass of the renderer.
    /// </summary>
    internal IPipelineProvider ForwardPass => this._forwardPass;

    /// <summary>
    /// Gets the shadow map pass of the renderer.
    /// </summary>
    internal IPipelineProvider ShadowMapPass => this._shadowPass;

    /// <summary>
    /// Gets the <see cref="SceneStorage"/> of the renderer.
    /// </summary>
    internal SceneStorage Storage { get; }

    /// <summary>
    /// Gets the current Veldrid's GraphicsDevice.
    /// </summary>
    internal GraphicsDevice GraphicsDevice { get; }

    /// <summary>
    /// Gets the main render texture.
    /// </summary>
    public RenderTexture MainRenderTexture { get; }

    /// <summary>
    /// Gets the current backend used by the renderer.
    /// </summary>
    public GraphicsBackend BackendType => this.GraphicsDevice.BackendType;

    /// <summary>
    /// Gets the <see cref="RenderSettings"/> used by the renderer.
    /// </summary>
    public RenderSettings Settings { get; }

    /// <summary>
    /// Gets the currently selected <see cref="RenderNode3D"/> under the mouse.
    /// </summary>
    public RenderNode3D? SelectedRenderNode => this._mousePickerPass.SelectedRenderNode;

    private readonly SwapchainRenderTexture _fullScreenRenderTexture;
    private readonly ResourceFactory _factory;
    private readonly FullScreenPass _fullScreenPass;
    private readonly GizmosPass _gizmosPass;
    private readonly Fence _fence;
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
    private readonly Dictionary<ResourceLayoutDescription, ResourceLayout> _resourceLayoutCache = new();
    private readonly SwapPopList<Renderable> _renderables = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Renderer"/> class.
    /// </summary>
    /// <param name="window">The window to render to.</param>
    /// <param name="graphicsBackend">The graphics backend to use.</param>
    /// <exception cref="InvalidOperationException">Thrown if the renderer is already initialized.</exception>
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

        this.Storage = new SceneStorage(this);

        this.Settings = new RenderSettings(this);

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

        this.Settings.PropertyChanged += this.Settings_PropertyChanged;
        Texture.InitializeDefaultTextures();
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Force recreation of pipeline for all renderables.
        foreach (var renderable in this._renderables)
        {
            renderable.InvalidatePipeline();
        }
    }

    public int SpritePassDrawCallCount => this._spritesPass.DrawCallCount;

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

    internal Renderable MakeRenderable(int instanceDataBlockSize)
    {
        var renderable = new Renderable(this, instanceDataBlockSize);
        this._renderables.Add(renderable);
        return renderable;
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
        this.RenderStarted?.Invoke(this, EventArgs.Empty);

        scene.OnBeforeRender();
        scene.RenderImGui();
        scene.UpdateSceneDirtyTransforms();

        this._commandList.Begin();
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

        this.RenderEnded?.Invoke(this, EventArgs.Empty);
    }

    public IntPtr GetOrCreateImGuiBinding(Texture texture)
    {
        return this._imGuiPass.GetOrCreateBinding(texture);
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

    /// <summary>
    /// Resizes the main render texture.
    /// </summary>
    /// <param name="width">The new width of the render texture.</param>
    /// <param name="height">The new height of the render texture.</param>
    /// <param name="viewportWidth">The width of the viewport.</param>
    /// <param name="viewportHeight">The height of the viewport.</param>
    public void Resize(uint width, uint height, uint viewportWidth, uint viewportHeight)
    {
        this.GraphicsDevice.ResizeMainWindow(width, height);
        this.GraphicsDevice.WaitForIdle();
        this._fullScreenRenderTexture.Resize(width, height);
        this.MainRenderTexture.Resize(viewportWidth, viewportHeight);
        this._imGuiPass.Resize(viewportWidth, viewportHeight);
    }

    /// <summary>
    /// Disposes the renderer.
    /// </summary>
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
}