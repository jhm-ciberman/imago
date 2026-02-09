using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LifeSim.Imago.Assets.Materials;
using LifeSim.Imago.Assets.Textures;
using LifeSim.Imago.Rendering.Internals;
using LifeSim.Imago.Rendering.Internals.Buffers;
using LifeSim.Imago.Rendering.Passes;
using LifeSim.Imago.Rendering.Sprites;
using LifeSim.Imago.SceneGraph;
using LifeSim.Imago.Startup;
using LifeSim.Imago.Utilities;
using Veldrid;
using Veldrid.Sdl2;
using Texture = LifeSim.Imago.Assets.Textures.Texture;
using Viewport = LifeSim.Imago.SceneGraph.Viewport;

namespace LifeSim.Imago.Rendering;

/// <summary>
/// The main engine renderer. This class is responsible for rendering the scene to the screen or to a texture
/// and orchestrating the rendering passes.
/// </summary>
public class Renderer : IDisposable
{
    /// <summary>
    /// Gets the global instance of the renderer.
    /// </summary>
    public static Renderer Instance { get; private set; } = null!;

    /// <summary>
    /// Creates a new graphics device.
    /// </summary>
    /// <param name="window">The window to render to.</param>
    /// <param name="graphicsBackend">The graphics backend to use.</param>
    /// <param name="debug">Whether to enable debug mode in the graphics device.</param>
    /// <returns>The created graphics device.</returns>
    public static GraphicsDevice CreateGraphicsDevice(Sdl2Window window, GraphicsBackend? graphicsBackend = null, bool debug = true)
    {
        GraphicsDeviceOptions options = new GraphicsDeviceOptions(
            debug: debug,
            swapchainDepthFormat: null, // no default depth buffer, we are doing a full screen final pass
            syncToVerticalBlank: true,
            resourceBindingModel: ResourceBindingModel.Improved,
            preferDepthRangeZeroToOne: true,
            preferStandardClipSpaceYDirection: true,
            swapchainSrgbFormat: false
        );

        return VeldridStartup.CreateGraphicsDevice(window, options, graphicsBackend ?? VeldridStartup.GetPlatformDefaultBackend());
    }

    /// <summary>
    /// Gets the current backend used by the renderer.
    /// </summary>
    public GraphicsBackend BackendType => this.GraphicsDevice.BackendType;

    /// <summary>
    /// Gets the main Viewport.
    /// </summary>
    public Viewport MainViewport { get; }

    /// <summary>
    /// Gets the GUI Viewport.
    /// </summary>
    public Viewport GuiViewport { get; }

    /// <summary>
    /// Gets the current Veldrid's GraphicsDevice.
    /// </summary>
    internal GraphicsDevice GraphicsDevice { get; }

    /// <summary>
    /// Gets the main render texture.
    /// </summary>
    internal RenderTexture MainRenderTexture { get; }

    /// <summary>
    /// Gets the GUI render texture.
    /// </summary>
    internal RenderTexture GuiRenderTexture { get; }

    /// <summary>
    /// Gets the full screen render texture.
    /// </summary>
    internal SwapchainRenderTexture FullScreenRenderTexture { get; }

    private readonly CommandList _commandList;
    private readonly DisposeCollector _disposeCollector;
    private readonly FullScreenPass _fullScreenPass;
    private readonly FullScreenPass _fullScreenPixelArtPass;
    private readonly Forward3DRenderer _forward3DRenderer;

    private readonly SpritesPass _spritesPass;
    private readonly ImGuiPass _imGuiPass;

    private readonly RendererResources _rendererResources;
    private readonly HashSet<IDisposable> _disposables = new HashSet<IDisposable>();
    private readonly Dictionary<ResourceLayoutDescription, ResourceLayout> _resourceLayoutCache = new();
    private float _totalTime;

    /// <summary>
    /// Gets the per-frame rendering statistics.
    /// </summary>
    public RenderStatistics Statistics { get; } = new RenderStatistics();

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
        {
            throw new InvalidOperationException("Only one instance of Renderer can be created.");
        }

        Instance = this;

        this.GraphicsDevice = gd;
        this._rendererResources = new RendererResources(this.GraphicsDevice);

        swapchain ??= this.GraphicsDevice.MainSwapchain;

        var framebuffer = swapchain.Framebuffer;

        this.MainViewport = new Viewport(new Vector2(framebuffer.Width, framebuffer.Height));
        this.GuiViewport = new Viewport(new Vector2(framebuffer.Width, framebuffer.Height));

        this.FullScreenRenderTexture = new SwapchainRenderTexture(gd, swapchain);
        this.MainRenderTexture = new RenderTexture(framebuffer.Width, framebuffer.Height);
        this.GuiRenderTexture = new RenderTexture(framebuffer.Width, framebuffer.Height);

        this._disposeCollector = new DisposeCollector();

        this._forward3DRenderer = new Forward3DRenderer(this);
        this._imGuiPass = new ImGuiPass(this);
        this._spritesPass = new SpritesPass(this);
        this._fullScreenPass = new FullScreenPass(this, isPixelArt: false);
        this._fullScreenPixelArtPass = new FullScreenPass(this, isPixelArt: true);



        var factory = this.GraphicsDevice.ResourceFactory;
        this._commandList = factory.CreateCommandList();
    }

    /// <summary>
    /// Gets the total elapsed time since the renderer was created, in seconds.
    /// </summary>
    internal float TotalTime => this._totalTime;

    /// <summary>
    /// Updates the state of the renderer and its components.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last frame, in seconds.</param>
    /// <param name="inputSnapshot">The current input snapshot.</param>
    public void Update(float deltaTime, InputSnapshot inputSnapshot)
    {
        this._totalTime += deltaTime;
        this._imGuiPass.Update(deltaTime, inputSnapshot);
        this._forward3DRenderer.Update(inputSnapshot);
    }

    /// <summary>
    /// Gets the resource layout for transform data.
    /// </summary>
    internal ResourceLayout TransformResourceLayout => this._rendererResources.TransformResourceLayout;

    /// <summary>
    /// Gets the resource layout for instance data.
    /// </summary>
    internal ResourceLayout InstanceResourceLayout => this._rendererResources.InstanceResourceLayout;

    /// <summary>
    /// Gets the resource layout for skeleton data.
    /// </summary>
    internal ResourceLayout SkeletonResourceLayout => this._rendererResources.SkeletonResourceLayout;

    /// <summary>
    /// Requests a data block for instance data.
    /// </summary>
    /// <param name="instanceDataBlockSize">The size of the instance data block.</param>
    internal DataBlock RequestInstanceDataBlock(int instanceDataBlockSize)
    {
        return this._rendererResources.RequestInstanceDataBlock(instanceDataBlockSize);
    }

    /// <summary>
    /// Requests a data block for transform data.
    /// </summary>
    internal DataBlock RequestTransformDataBlock()
    {
        return this._rendererResources.RequestTransformDataBlock();
    }

    /// <summary>
    /// Requests a data block for skeleton data.
    /// </summary>
    internal DataBlock RequestSkeletonDataBlock()
    {
        return this._rendererResources.RequestSkeletonDataBlock();
    }

    /// <summary>
    /// Gets or creates an ImGui binding for the given texture.
    /// </summary>
    /// <param name="texture">The texture to get or create the binding for.</param>
    /// <returns>The ImGui binding.</returns>
    public nint GetOrCreateImGuiBinding(Texture texture)
    {
        return this._imGuiPass.GetOrCreateBinding(texture);
    }

    /// <summary>
    /// Creates a new surface material of the specified type.
    /// </summary>
    /// <typeparam name="T">The material type, must have a <see cref="SurfaceShaderAttribute"/>.</typeparam>
    /// <returns>A new material instance.</returns>
    public T MakeMaterial<T>() where T : Material, ICreateableMaterial<T>
    {
        return this._forward3DRenderer.MakeMaterial<T>();
    }

    /// <summary>
    /// Schedules a disposable resource to be released when the GPU is idle.
    /// </summary>
    /// <param name="disposable">The object to dispose.</param>
    public void DisposeWhenIdle(IDisposable disposable)
    {
        this._disposeCollector.Add(disposable);
    }

    /// <summary>
    /// Renders a <see cref="Stage"/> to the main swapchain.
    /// </summary>
    /// <param name="stage">The stage to render.</param>
    public void Render(Stage stage)
    {
        var stats = this.Statistics;
        stats.RenderTime.Begin();
        stats.BeginFrame();

        stats.PreparePhase.Begin();
        stage.PrepareForRender(this.MainRenderTexture);
        var cl = this._commandList;
        cl.Begin();
        this._rendererResources.Update(cl);
        stats.PreparePhase.End();

        stats.Pass3D.Begin();
        var layer3D = stage.Layer3D;
        if (layer3D != null && layer3D.IsVisible)
        {
            this._forward3DRenderer.Render(cl, layer3D, this.MainRenderTexture);
        }
        else
        {
            cl.SetFramebuffer(this.MainRenderTexture.Framebuffer);
            var col = stage.DefaultClearColor;
            cl.ClearColorTarget(0, new RgbaFloat(col.R / 255f, col.G / 255f, col.B / 255f, col.A / 255f));
            cl.ClearDepthStencil(1f);
        }
        stats.Pass3D.End();

        stats.Pass2D.Begin();
        cl.SetFramebuffer(this.GuiRenderTexture.Framebuffer);
        cl.ClearColorTarget(0, RgbaFloat.Clear);
        cl.ClearDepthStencil(1f);

        foreach (var layer in stage.Layers)
        {
            if (layer is ILayer2D layer2D && layer2D.IsVisible)
            {
                this._spritesPass.Render(cl, this.GuiRenderTexture, layer2D);
            }
        }

        this._fullScreenPass.Render(cl, this.MainRenderTexture);
        this._fullScreenPixelArtPass.Render(cl, this.GuiRenderTexture);
        this._imGuiPass.Render(cl);
        cl.End();
        stats.Pass2D.End();

        stats.GpuSync.Begin();
        this.GraphicsDevice.WaitForIdle();
        this.GraphicsDevice.SubmitCommands(cl);
        this._disposeCollector.DisposeAll();
        stats.GpuSync.End();

        this._forward3DRenderer.ReadPickingResult(stage.Layer3D);

        stats.RenderTime.End();

        if (this.GraphicsDevice.MainSwapchain != null)
        {
            this.GraphicsDevice.SwapBuffers();
        }
    }


    /// <summary>
    /// Renders a <see cref="Layer3D"/> to an off-screen texture.
    /// </summary>
    /// <param name="layer">The 3D layer to render.</param>
    /// <param name="renderTexture">The render texture to render to.</param>
    /// <param name="resolvedTexture">The texture to resolve the multisampled result to. If null, no resolving is performed.</param>
    public void RenderToOffScreenTexture(Layer3D layer, RenderTexture renderTexture, Texture? resolvedTexture = null)
    {
        var cl = this._commandList;

        layer.PrepareForRender(renderTexture);

        cl.Begin();
        this._rendererResources.Update(cl);
        this._forward3DRenderer.Render(cl, layer, renderTexture);

        //if (renderTexture.SampleCount != TextureSampleCount.Count1 && resolvedTexture != null)
        //{
        //    cl.ResolveTexture(renderTexture.ForwardColorTexture, resolvedTexture!.VeldridTexture);
        //}

        cl.End();

        this.GraphicsDevice.WaitForIdle();

        this.GraphicsDevice.SubmitCommands(cl);
    }

    /// <summary>
    /// Resolves a multisampled render texture into a non-multisampled texture.
    /// </summary>
    /// <remarks>
    /// If the source render texture is not multisampled, this method performs a copy.
    /// </remarks>
    /// <param name="renderTexture">The multisampled render texture to resolve.</param>
    /// <param name="resolvedTexture">The target non-multisampled texture.</param>
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

    /// <summary>
    /// Gets or creates a cached <see cref="ResourceLayout"/> for the given description.
    /// </summary>
    /// <param name="description">The description of the resource layout.</param>
    /// <returns>The cached or newly created resource layout.</returns>
    public ResourceLayout GetResourceLayout(ResourceLayoutDescription description)
    {
        if (!this._resourceLayoutCache.TryGetValue(description, out ResourceLayout? layout))
        {
            layout = this.GraphicsDevice.ResourceFactory.CreateResourceLayout(description);
            this._resourceLayoutCache.Add(description, layout);
        }
        return layout;
    }

    /// <summary>
    /// Resizes the main viewport and all related textures.
    /// </summary>
    /// <param name="width">The new width of the viewport.</param>
    /// <param name="height">The new height of the viewport.</param>
    public void Resize(uint width, uint height)
    {
        this.GraphicsDevice.ResizeMainWindow(width, height);
        this.GraphicsDevice.WaitForIdle();
        this.FullScreenRenderTexture.Resize(width, height);
        this.MainRenderTexture.Resize(width, height);
    }

    /// <summary>
    /// Resizes the GUI viewport and its related textures.
    /// </summary>
    /// <param name="width">The new width of the GUI viewport.</param>
    /// <param name="height">The new height of the GUI viewport.</param>
    public void ResizeGui(uint width, uint height)
    {
        this.GuiRenderTexture.Resize(width, height);
    }

    /// <summary>
    /// Registers a disposable object to be disposed when the renderer is disposed.
    /// </summary>
    /// <param name="disposable">The object to register.</param>
    internal void RegisterDisposable(IDisposable disposable)
    {
        this._disposables.Add(disposable);
    }

    /// <summary>
    /// Unregisters a disposable object.
    /// </summary>
    /// <param name="disposable">The object to unregister.</param>
    internal void UnregisterDisposable(IDisposable disposable)
    {
        this._disposables.Remove(disposable);
    }

    /// <summary>
    /// Notifies the renderer that the given texture is dirty and needs to be updated.
    /// </summary>
    /// <param name="texture">The texture to update.</param>
    internal void NotifyTextureDirty(Texture texture)
    {
        this._rendererResources.NotifyTextureDirty(texture);
    }

    /// <summary>
    /// Notifies the renderer that the given material's resources are dirty and need to be updated.
    /// </summary>
    /// <param name="material">The material to update.</param>
    internal void NotifyMaterialResourcesDirty(Material material)
    {
        this._rendererResources.NotifyMaterialResourcesDirty(material);
    }

    /// <summary>
    /// Gets a value identifying whether texture coordinates begin in the top left corner of a Texture.
    /// If true, (0, 0) refers to the top-left texel of a Texture. If false, (0, 0) refers to the bottom-left
    /// texel of a Texture. This property is useful for determining how the output of a Framebuffer should be sampled.
    /// </summary>
    public bool IsUvOriginTopLeft => this.GraphicsDevice.IsUvOriginTopLeft;

    /// <summary>
    /// Disposes the renderer.
    /// </summary>
    public void Dispose()
    {
        if (Instance == null) return;

        Console.WriteLine("Disposing renderer.");
        try
        {
            this.GraphicsDevice.WaitForIdle();
            this.FullScreenRenderTexture.Dispose();
            this.MainRenderTexture.Dispose();
            this.GuiRenderTexture.Dispose();
            this._commandList.Dispose();

            foreach (var disposable in this._disposables.ToArray())
            {
                disposable.Dispose();
            }

            // Passes
            this._imGuiPass.Dispose();
            this._spritesPass.Dispose();
            this._fullScreenPass.Dispose();
            this._forward3DRenderer.Dispose();
            this._rendererResources.Dispose();

            Console.WriteLine("All resources disposed. Attempting to dispose graphics device.");
            // The last thing to dispose is the graphics device.
            // Otherwise AccessViolationException is thrown.
            this.GraphicsDevice.Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error disposing renderer:");
            Console.WriteLine(e);
#if DEBUG
            throw; // We want to fix any issues in debug mode.
#endif
        }
        finally
        {
            Instance = null!;
        }
    }
}
