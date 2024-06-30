using System;
using System.Collections.Generic;
using System.Linq;
using LifeSim.Imago.Graphics.Materials;
using LifeSim.Imago.Graphics.Textures;
using LifeSim.Imago.SceneGraph;
using LifeSim.Support.Drawing;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;
using Texture = LifeSim.Imago.Graphics.Textures.Texture;
using Viewport = LifeSim.Imago.SceneGraph.Viewport;

namespace LifeSim.Imago.Graphics.Rendering;

public class Renderer : IDisposable
{
    public const int MIN_BUFFER_BLOCKS = 1024;

    /// <summary>
    /// Gets the global instance of the renderer.
    /// </summary>
    public static Renderer Instance { get; private set; } = null!;

    /// <summary>
    /// Occurs when the viewport is resized.
    /// </summary>
    public event EventHandler<ViewportResizedEventArgs>? ViewportResized;

    /// <summary>
    /// Gets the current Veldrid's GraphicsDevice.
    /// </summary>
    internal GraphicsDevice GraphicsDevice { get; }

    /// <summary>
    /// Gets the main render texture.
    /// </summary>
    public RenderTexture MainRenderTexture { get; }

    /// <summary>
    /// Gets the full screen render texture.
    /// </summary>
    internal SwapchainRenderTexture FullScreenRenderTexture => this._fullScreenRenderTexture;

    /// <summary>
    /// Gets the current backend used by the renderer.
    /// </summary>
    public GraphicsBackend BackendType => this.GraphicsDevice.BackendType;

    /// <summary>
    /// Gets the main Viewport.
    /// </summary>
    public Viewport Viewport { get; }

    private readonly SwapchainRenderTexture _fullScreenRenderTexture;
    private readonly ResourceFactory _factory;
    private readonly Fence _fence;
    private readonly CommandList _commandList;
    private readonly CommandList _fullScreenCommandList;
    private readonly DisposeCollector _disposeCollector;
    private readonly Dictionary<ResourceLayoutDescription, ResourceLayout> _resourceLayoutCache = new();

    private readonly List<DataBuffer> _instanceDataBuffers = new List<DataBuffer>();
    private readonly List<DataBuffer> _transformDataBuffers = new List<DataBuffer>();
    private readonly List<DataBuffer> _skeletonDataBuffers = new List<DataBuffer>();

    private readonly List<Texture> _dirtyTextures = new();
    private readonly List<Material> _dirtyMaterials = new();

    public ResourceLayout TransformResourceLayout { get; }
    public ResourceLayout InstanceResourceLayout { get; }
    public ResourceLayout SkeletonResourceLayout { get; }

    private readonly List<Skeleton> _skeletons = new List<Skeleton>();
    private readonly HashSet<IDisposable> _disposables = new HashSet<IDisposable>();

    private readonly FullScreenPass _fullScreenPass;
    private readonly GizmosPass _gizmosPass;
    private readonly ForwardPass _forwardPass;
    private readonly ImmediatePass _immediatePass;
    private readonly ShadowPass _shadowPass;
    private readonly SpritesPass _spritesPass;
    private readonly ImGuiPass _imGuiPass;
    private readonly MousePickingPass _mousePickerPass;
    private readonly ParticlesPass _particlesPass;
    private readonly SkyDomePass _skyDomePass;


    public static GraphicsDeviceOptions GetGraphicsDeviceOptions(bool debug = false)
    {
        return new GraphicsDeviceOptions(
            debug: true, //debug,
            swapchainDepthFormat: null, //PixelFormat.R16_UNorm,
            syncToVerticalBlank: false,
            resourceBindingModel: ResourceBindingModel.Improved,
            preferDepthRangeZeroToOne: true,
            preferStandardClipSpaceYDirection: true,
            swapchainSrgbFormat: false
        );
    }

    public static GraphicsDevice CreateGraphicsDevice(Sdl2Window window, GraphicsBackend? graphicsBackend = null, bool debug = false)
    {
        GraphicsDeviceOptions options = GetGraphicsDeviceOptions(debug);

        return VeldridStartup.CreateGraphicsDevice(window, options, graphicsBackend ?? VeldridStartup.GetPlatformDefaultBackend());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Renderer"/> class.
    /// </summary>
    /// <param name="window">The window to render to.</param>
    /// <param name="graphicsBackend">The graphics backend to use.</param>
    /// <param name="debug">Whether to enable debug mode in the graphics device.</param>
    /// <exception cref="InvalidOperationException">Thrown if the renderer is already initialized.</exception>
    public Renderer(Sdl2Window window, GraphicsBackend? graphicsBackend = null, bool debug = false)
        : this(CreateGraphicsDevice(window, graphicsBackend, debug)) { }


    /// <summary>
    /// Initializes a new instance of the <see cref="Renderer"/> class.
    /// </summary>
    /// <param name="gd">The graphics device to use.</param>
    /// <param name="swapchain">The swapchain to use. If null, the main swapchain of the graphics device is used.</param>
    /// <exception cref="InvalidOperationException">Thrown if the renderer is already initialized.</exception>
    public Renderer(GraphicsDevice gd, Swapchain? swapchain = null)
    {
        if (Instance != null)
            throw new InvalidOperationException("Only one instance of Renderer can be created.");

        Instance = this;

        this.GraphicsDevice = gd;
        swapchain ??= this.GraphicsDevice.MainSwapchain;

        this.Viewport = new Viewport(swapchain.Framebuffer.Width, swapchain.Framebuffer.Height);

        this._disposeCollector = new DisposeCollector();
        this._factory = this.GraphicsDevice.ResourceFactory;

        this._fullScreenRenderTexture = new SwapchainRenderTexture(gd, swapchain);

        this.MainRenderTexture = new RenderTexture(swapchain.Framebuffer.Width, swapchain.Framebuffer.Height);

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

        this._imGuiPass = new ImGuiPass(this, this.MainRenderTexture);
        this._mousePickerPass = new MousePickingPass(this, this.MainRenderTexture);
        this._gizmosPass = new GizmosPass(this, this.MainRenderTexture);
        this._particlesPass = new ParticlesPass(this, this.MainRenderTexture);
        this._shadowPass = new ShadowPass(this);
        this._forwardPass = new ForwardPass(this, this.MainRenderTexture, this._shadowPass);
        this._immediatePass = new ImmediatePass(this, this.MainRenderTexture);
        this._spritesPass = new SpritesPass(this, this.MainRenderTexture);
        this._skyDomePass = new SkyDomePass(this, this.MainRenderTexture);
        this._fullScreenPass = new FullScreenPass(this, this.MainRenderTexture, this.FullScreenRenderTexture);

        this._commandList = this._factory.CreateCommandList();

        this._fullScreenCommandList = this._factory.CreateCommandList();

        this._fence = this._factory.CreateFence(false);
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

    public Material MakeMaterial()
    {
        return new Material(this._forwardPass.DefaultShader, this._shadowPass.DefaultShader, this._mousePickerPass.DefaultShader);
    }

    public void DisposeWhenIdle(IDisposable disposable)
    {
        this._disposeCollector.Add(disposable);
    }

    public void DisposeWhenIdle(IDisposable[] disposables)
    {
        foreach (var disposable in disposables)
        {
            this.DisposeWhenIdle(disposable);
        }
    }

    /// <summary>
    /// Renders the scene to the screen.
    /// </summary>
    /// <param name="stage">The stage to render.</param>
    public void Render(Stage stage)
    {
        var cl = this._commandList;
        cl.Begin();
        this.RenderCore(cl, stage, this.MainRenderTexture);
        cl.End();

        this.GraphicsDevice.WaitForIdle();
        this._fence.Reset();

        this.GraphicsDevice.SubmitCommands(cl, this._fence);

        cl = this._fullScreenCommandList;
        cl.Begin();
        this._fullScreenPass.Render(cl);
        cl.End();

        this.GraphicsDevice.SubmitCommands(cl, null);

        this._disposeCollector.DisposeAll();

        if (this.GraphicsDevice.MainSwapchain != null)
            this.GraphicsDevice.SwapBuffers();
    }


    /// <summary>
    /// Renders the scene to the given render texture. If the render texture is multisampled, the result can be resolved to the given texture.
    /// </summary>
    /// <param name="stage">The stage to render.</param>
    /// <param name="renderTexture">The render texture to render to.</param>
    /// <param name="resolvedTexture">The texture to resolve to. If null, the render texture is not resolved.</param>
    public void RenderToOffScreenTexture(Stage stage, RenderTexture renderTexture, Texture? resolvedTexture = null)
    {
        var cl = this._commandList;

        cl.Begin();

        this.RenderCore(cl, stage, renderTexture);

        //if (renderTexture.SampleCount != TextureSampleCount.Count1 && resolvedTexture != null)
        //{
        //    cl.ResolveTexture(renderTexture.ForwardColorTexture, resolvedTexture!.VeldridTexture);
        //}

        cl.End();

        this.GraphicsDevice.WaitForIdle();

        this.GraphicsDevice.SubmitCommands(cl);
    }

    public void ResolveTexture(RenderTexture renderTexture, Texture resolvedTexture)
    {

        var cl = this._commandList;

        cl.Begin();

        if (renderTexture.SampleCount == TextureSampleCount.Count1)
        {
            // Nothing to resolve, just copy the texture.
            cl.CopyTexture(renderTexture.ForwardColorTexture, resolvedTexture.VeldridTexture);
        }
        else
        {
            cl.ResolveTexture(renderTexture.ForwardColorTexture, resolvedTexture.VeldridTexture);
        }

        cl.End();

        this.GraphicsDevice.SubmitCommands(cl);
    }

    private void RenderCore(CommandList cl, Stage stage, RenderTexture renderTexture)
    {
        if (stage.MultiSampleCount != renderTexture.SampleCount)
            stage.MultiSampleCount = renderTexture.SampleCount; // It's a hack to invalidate the pipeline of all renderables, but whatever.

        stage.PrepareForRender();

        this.UpdateBuffers(cl);

        this._shadowPass.Render(cl, stage);

        cl.SetFramebuffer(renderTexture.Framebuffer);
        ClearRenderTarget(cl, stage.Scene);

        this._forwardPass.Render(cl, stage, renderTexture);
        this._immediatePass.Render(cl, stage, renderTexture);
        this._skyDomePass.Render(cl, stage, renderTexture);
        this._mousePickerPass.Render(cl, stage, renderTexture);
        this._particlesPass.Render(cl, stage, renderTexture);
        this._gizmosPass.Render(cl, stage, renderTexture);
        this._spritesPass.Render(cl, stage, renderTexture);

        this._imGuiPass.Render(cl, renderTexture);
    }

    private static void ClearRenderTarget(CommandList cl, Scene scene)
    {
        ColorF? clearColor = scene.Camera?.ClearColor ?? scene.ClearColor;
        if (clearColor != null)
        {
            var col = clearColor.Value;
            cl.ClearColorTarget(0, new RgbaFloat(col.R, col.G, col.B, col.A));
            cl.ClearDepthStencil(1f);
        }
    }

    public ResourceLayout GetResourceLayout(ResourceLayoutDescription description)
    {
        if (!this._resourceLayoutCache.TryGetValue(description, out ResourceLayout? layout))
        {
            layout = this._factory.CreateResourceLayout(description);
            this._resourceLayoutCache.Add(description, layout);
        }
        return layout;
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

    internal DataBlock RequestTransformDataBlock()
    {
        for (int i = 0; i < this._transformDataBuffers.Count; i++)
        {
            var buffer = this._transformDataBuffers[i];
            if (!buffer.IsFull)
                return buffer.RequestBlock();
        }

        var newBuffer = new DataBuffer(this.GraphicsDevice, MIN_BUFFER_BLOCKS, 64, this.TransformResourceLayout);
        newBuffer.Name = "TransformDataBuffer " + this._transformDataBuffers.Count;
        this._transformDataBuffers.Add(newBuffer);
        return newBuffer.RequestBlock();
    }

    internal DataBlock RequestInstanceDataBlock(int instanceDataBlockSize)
    {
        for (int i = 0; i < this._instanceDataBuffers.Count; i++)
        {
            var buffer = this._instanceDataBuffers[i];
            if (buffer.BlockSize == instanceDataBlockSize && !buffer.IsFull)
                return buffer.RequestBlock();
        }

        var newBuffer = new DataBuffer(this.GraphicsDevice, MIN_BUFFER_BLOCKS, instanceDataBlockSize, this.InstanceResourceLayout);
        newBuffer.Name = "InstanceDataBuffer " + this._instanceDataBuffers.Count;
        this._instanceDataBuffers.Add(newBuffer);
        return newBuffer.RequestBlock();
    }

    internal void RegisterSkeleton(Skeleton skeleton)
    {
        this._skeletons.Add(skeleton);
    }

    internal void UnregisterSkeleton(Skeleton skeleton)
    {
        this._skeletons.Remove(skeleton);
    }

    internal void RegisterDisposable(IDisposable disposable)
    {
        this._disposables.Add(disposable);
    }

    internal void UnregisterDisposable(IDisposable disposable)
    {
        this._disposables.Remove(disposable);
    }

    internal DataBlock RequestSkeletonDataBlock()
    {
        for (int i = 0; i < this._skeletonDataBuffers.Count; i++)
        {
            var buffer = this._skeletonDataBuffers[i];
            if (!buffer.IsFull)
                return buffer.RequestBlock();
        }

        var newBuffer = new DataBuffer(this.GraphicsDevice, MIN_BUFFER_BLOCKS / Skeleton.MAX_NUMBER_OF_BONES, Skeleton.MAX_NUMBER_OF_BONES * 64, this.SkeletonResourceLayout);
        newBuffer.Name = "SkeletonDataBuffer " + this._skeletonDataBuffers.Count;
        this._skeletonDataBuffers.Add(newBuffer);
        return newBuffer.RequestBlock();
    }

    internal void UpdateBuffers(CommandList commandList)
    {
        for (int i = 0; i < this._instanceDataBuffers.Count; i++)
        {
            this._instanceDataBuffers[i].UploadToGPU(commandList);
        }

        for (int i = 0; i < this._transformDataBuffers.Count; i++)
        {
            this._transformDataBuffers[i].UploadToGPU(commandList);
        }

        foreach (var skeleton in this._skeletons)
        {
            skeleton.Update();
        }

        for (int i = 0; i < this._skeletonDataBuffers.Count; i++)
        {
            this._skeletonDataBuffers[i].UploadToGPU(commandList);
        }

        if (this._dirtyMaterials.Count > 0)
        {
            foreach (var material in this._dirtyMaterials)
            {
                material.Update(this._factory);
            }
            this._dirtyMaterials.Clear();
        }

        if (this._dirtyTextures.Count > 0)
        {
            foreach (var resource in this._dirtyTextures)
            {
                resource.Update(this.GraphicsDevice, commandList);
            }
            this._dirtyTextures.Clear();
        }
    }

    internal void NotifyTextureDirty(Texture texture)
    {
        this._dirtyTextures.Add(texture);
    }

    internal void NotifyMaterialResourcesDirty(Material material)
    {

        this._dirtyMaterials.Add(material);
    }

    /// <summary>
    /// Disposes the renderer.
    /// </summary>
    public void Dispose()
    {
        Console.WriteLine("Disposing renderer.");
        try
        {
            this.GraphicsDevice.WaitForIdle();
            this._fullScreenRenderTexture.Dispose();
            this.MainRenderTexture.Dispose();
            this._commandList.Dispose();
            this._fence.Dispose();

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

            foreach (var disposable in this._disposables.ToArray())
            {
                Console.WriteLine("Disposing " + disposable.GetType().Name);
                disposable.Dispose();
            }

            // Passes
            this._imGuiPass.Dispose();
            this._mousePickerPass.Dispose();
            this._gizmosPass.Dispose();
            this._particlesPass.Dispose();
            this._shadowPass.Dispose();
            this._forwardPass.Dispose();
            this._immediatePass.Dispose();
            this._spritesPass.Dispose();
            this._skyDomePass.Dispose();
            this._fullScreenPass.Dispose();

            Console.WriteLine("All resources disposed. Attempting to dispose graphics device.");
            // The last thing to dispose is the graphics device.
            // Otherwise AccessViolationException is thrown.
            this.GraphicsDevice.Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error disposing renderer:");
            Console.WriteLine(e);
        }
        finally
        {
            Instance = null!;
        }
    }
}
