using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.InteropServices;
using FontStashSharp.Interfaces;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;
using LifeSim.Support;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering;

public partial class Renderer : IDisposable
{
    public const int MIN_BUFFER_BLOCKS = 1024;

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
    /// Event that is raised when the viewport is resized.
    /// </summary>
    public event EventHandler<ViewportResizedEventArgs>? ViewportResized;

    /// <summary>
    /// Get the forward pass of the renderer.
    /// </summary>
    internal IPipelineProvider ForwardPass => this._forwardPass;

    /// <summary>
    /// Gets the shadow map pass of the renderer.
    /// </summary>
    internal IPipelineProvider ShadowMapPass => this._shadowPass;

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

    private readonly List<DataBuffer> _instanceDataBuffers = new List<DataBuffer>();
    private readonly List<DataBuffer> _transformDataBuffers = new List<DataBuffer>();
    private readonly List<DataBuffer> _skeletonDataBuffers = new List<DataBuffer>();

    private readonly List<Texture> _dirtyTextures = new();
    private readonly List<MaterialBase> _dirtyMaterials = new();
    private readonly List<Renderable> _dirtyRenderables = new();

    public ResourceLayout TransformResourceLayout { get; }
    public ResourceLayout InstanceResourceLayout { get; }
    public ResourceLayout SkeletonResourceLayout { get; }

    private readonly List<Skeleton> _skeletons = new List<Skeleton>();

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

        this.InstanceResourceLayout = this._factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("InstanceDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));
        this.InstanceResourceLayout.Name = "InstanceData Resource Layout";

        this.TransformResourceLayout = this._factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("TransformDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));
        this.TransformResourceLayout.Name = "TransformData Resource Layout";

        this.SkeletonResourceLayout = this._factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("BonesDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));
        this.SkeletonResourceLayout.Name = "BonesData Resource Layout";

        Renderable.PipelineDirty += this.OnRenderablePipelineDirty;
        MaterialBase.MaterialResourceSetDirty += this.OnMaterialResourceSetDirty;
        Texture.TextureDirty += this.OnTextureDirty;

        this.Settings = new RenderSettings(this);

        this._imGuiPass = new ImGuiPass(this, this.MainRenderTexture);
        this._mousePickerPass = new MousePickingPass(this, this.MainRenderTexture);
        this._gizmosPass = new GizmosPass(this, this.MainRenderTexture);
        this._particlesPass = new ParticlesPass(this, this.MainRenderTexture);
        this._shadowPass = new ShadowPass(this);
        this._forwardPass = new ForwardPass(this, this.MainRenderTexture, this._shadowPass);
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

    internal Renderable MakeRenderable<TInstanceData>() where TInstanceData : unmanaged
    {
        var transformDataBlock = this.RequestTransformDataBlock();
        var instanceDataBlock = this.RequestInstanceDataBlock(Marshal.SizeOf<TInstanceData>());
        var renderable = new Renderable(transformDataBlock, instanceDataBlock);
        this._renderables.Add(renderable);
        return renderable;
    }

    /// <summary>
    /// Updates the renderer.
    /// </summary>
    /// <param name="deltaTime">The time since the last update in seconds.</param>
    /// <param name="inputSnapshot">The current input state.</param>
    public void Update(float deltaTime, InputSnapshot inputSnapshot)
    {
        this._imGuiPass.Update(deltaTime, inputSnapshot);
        this._mousePickerPass.SetMousePosition(inputSnapshot.MousePosition);
    }

    /// <summary>
    /// Renders the scene.
    /// </summary>
    /// <param name="scene">The scene to render.</param>
    public void Render(Scene scene)
    {
        this.RenderStarted?.Invoke(this, EventArgs.Empty);

        scene.OnBeforeRender();
        scene.RenderImGui();
        scene.UpdateTransforms();

        this._commandList.Begin();
        this.UpdateBuffers(this._commandList);
        this.ClearRenderTarget(this._commandList, scene);
        this._commandList.End();

        this.RenderJobs(scene);

        this.GraphicsDevice.WaitForIdle();
        this._fence.Reset();
        this.GraphicsDevice.SubmitCommands(this._commandList, this._fence);

        this.SubmitJobs();

        this._disposeCollector.DisposeAll();
        this.GraphicsDevice.SwapBuffers();

        this.RenderEnded?.Invoke(this, EventArgs.Empty);
    }

    private void ClearRenderTarget(CommandList commandList, Scene scene)
    {
        commandList.SetFramebuffer(this.MainRenderTexture.Framebuffer);
        if (scene.BackgroundColor != null)
        {
            ColorF col = scene.BackgroundColor.Value;
            commandList.ClearColorTarget(0, new RgbaFloat(col.R, col.G, col.B, col.A));
        }
    }

    private void RenderJobs(Scene scene)
    {
        for (int i = 0; i < this._jobs.Count; i++)
        {
            this._jobs[i].Execute(scene);
        }
    }

    private void SubmitJobs()
    {
        for (int i = 0; i < this._jobs.Count; i++)
        {
            this._jobs[i].SubmitCommands(this.GraphicsDevice);
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

        this.ViewportResized?.Invoke(this, new ViewportResizedEventArgs(viewportWidth, viewportHeight));
    }

    private DataBlock RequestTransformDataBlock()
    {
        lock (this._transformDataBuffers)
        {
            for (int i = 0; i < this._transformDataBuffers.Count; i++)
            {
                var buffer = this._transformDataBuffers[i];
                if (!buffer.IsFull)
                {
                    return buffer.RequestBlock();
                }
            }

            var newBuffer = new DataBuffer(this.GraphicsDevice, MIN_BUFFER_BLOCKS, 64, this.TransformResourceLayout);
            newBuffer.Name = "TransformDataBuffer " + this._transformDataBuffers.Count;
            this._transformDataBuffers.Add(newBuffer);
            return newBuffer.RequestBlock();
        }
    }

    private DataBlock RequestInstanceDataBlock(int instanceDataBlockSize)
    {
        lock (this._instanceDataBuffers)
        {
            for (int i = 0; i < this._instanceDataBuffers.Count; i++)
            {
                var buffer = this._instanceDataBuffers[i];
                if (buffer.BlockSize == instanceDataBlockSize && !buffer.IsFull)
                {
                    return buffer.RequestBlock();
                }
            }

            var newBuffer = new DataBuffer(this.GraphicsDevice, MIN_BUFFER_BLOCKS, instanceDataBlockSize, this.InstanceResourceLayout);
            newBuffer.Name = "InstanceDataBuffer " + this._instanceDataBuffers.Count;
            this._instanceDataBuffers.Add(newBuffer);
            return newBuffer.RequestBlock();
        }
    }

    internal void RegisterSkeleton(Skeleton skeleton)
    {
        lock (this._skeletons)
        {
            this._skeletons.Add(skeleton);
        }
    }

    internal void UnregisterSkeleton(Skeleton skeleton)
    {
        lock (this._skeletons)
        {
            this._skeletons.Remove(skeleton);
        }
    }

    internal DataBlock RequestSkeletonDataBlock()
    {
        lock (this._skeletonDataBuffers)
        {
            for (int i = 0; i < this._skeletonDataBuffers.Count; i++)
            {
                var buffer = this._skeletonDataBuffers[i];
                if (!buffer.IsFull)
                {
                    return buffer.RequestBlock();
                }
            }

            var newBuffer = new DataBuffer(this.GraphicsDevice, MIN_BUFFER_BLOCKS / Skeleton.MAX_NUMBER_OF_BONES, Skeleton.MAX_NUMBER_OF_BONES * 64, this.SkeletonResourceLayout);
            newBuffer.Name = "SkeletonDataBuffer " + this._skeletonDataBuffers.Count;
            this._skeletonDataBuffers.Add(newBuffer);
            return newBuffer.RequestBlock();
        }
    }

    internal void UpdateBuffers(CommandList commandList)
    {
        lock (this._instanceDataBuffers)
        {
            for (int i = 0; i < this._instanceDataBuffers.Count; i++)
            {
                this._instanceDataBuffers[i].UploadToGPU(commandList);
            }
        }

        lock (this._transformDataBuffers)
        {
            for (int i = 0; i < this._transformDataBuffers.Count; i++)
            {
                this._transformDataBuffers[i].UploadToGPU(commandList);
            }
        }

        lock (this._skeletons)
        {
            foreach (var skeleton in this._skeletons)
            {
                skeleton.Update();
            }
        }

        lock (this._instanceDataBuffers)
        {
            for (int i = 0; i < this._skeletonDataBuffers.Count; i++)
            {
                this._skeletonDataBuffers[i].UploadToGPU(commandList);
            }
        }

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

        lock (this._dirtyTextures)
        {
            if (this._dirtyTextures.Count > 0)
            {
                foreach (var resource in this._dirtyTextures)
                {
                    resource.Update(this.GraphicsDevice, commandList);
                }
                this._dirtyTextures.Clear();
            }
        }

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

        for (int i = 0; i < this._instanceDataBuffers.Count; i++)
        {
            this._instanceDataBuffers[i].Dispose();
        }

        for (int i = 0; i < this._transformDataBuffers.Count; i++)
        {
            this._transformDataBuffers[i].Dispose();
        }

        for (int i = 0; i < this._skeletonDataBuffers.Count; i++)
        {
            this._skeletonDataBuffers[i].Dispose();
        }

        foreach (var skeleton in this._skeletons)
        {
            skeleton.Dispose();
        }
    }

    public void AddImmediateRenderNode(ImmediateRenderNode3D node)
    {
        this._forwardPass.AddImmediateRenderNode(node);
    }

    public void RemoveImmediateRenderNode(ImmediateRenderNode3D node)
    {
        this._forwardPass.RemoveImmediateRenderNode(node);
    }

    public IntPtr GetOrCreateImGuiBinding(Texture texture)
    {
        return this._imGuiPass.GetOrCreateBinding(texture);
    }
}