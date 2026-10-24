using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Imago.Assets.Materials;
using Imago.Assets.Textures;
using Imago.Input;
using Imago.Rendering.Internals;
using Imago.Rendering.Internals.Buffers;
using Imago.Rendering.Passes;
using Imago.Rendering.Passes.Shadows;
using Imago.SceneGraph;
using Imago.Support.Drawing;
using NeoVeldrid;
using Shader = Imago.Assets.Materials.Shader;

namespace Imago.Rendering;

/// <summary>
/// Orchestrates the rendering of a <see cref="Scene3DLayer"/> by coordinating various rendering passes.
/// </summary>
/// <remarks>
/// This class is responsible for setting up and executing the rendering pipeline for a 3D layer,
/// including shadow mapping, forward rendering, skybox, particles, and gizmos.
/// </remarks>
public class Forward3DRenderer : IDisposable
{
    /// <summary>
    /// Gets the global instance of the 3D renderer.
    /// </summary>
    public static Forward3DRenderer Instance { get; private set; } = null!;

    private readonly Renderer _renderer;
    private readonly RendererResources _resources;
    private readonly ParticlesPass _particlesPass;
    private readonly SkyDomePass _skyDomePass;
    private readonly GizmosPass _gizmosPass;
    private readonly ForwardPass _forwardPass;
    private readonly ShadowPass _shadowPass;
    private readonly MousePickingPass _mousePickerPass;
    private readonly ImmediatePass _immediatePass;
    private readonly AmbientOcclusionPass _ambientOcclusionPass;
    private readonly Dictionary<Type, ShaderSet> _shaderCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Forward3DRenderer"/> class.
    /// </summary>
    /// <param name="renderer">The main renderer instance.</param>
    /// <exception cref="InvalidOperationException">Thrown if a 3D renderer already exists.</exception>
    public Forward3DRenderer(Renderer renderer)
    {
        if (Instance != null)
        {
            throw new InvalidOperationException("Only one instance of Forward3DRenderer can be created.");
        }

        Instance = this;

        this._renderer = renderer;
        this._resources = new RendererResources(renderer.GraphicsDevice);
        this._mousePickerPass = new MousePickingPass(renderer, this);
        this._gizmosPass = new GizmosPass(renderer);
        this._particlesPass = new ParticlesPass(renderer);
        this._shadowPass = new ShadowPass(renderer, this);
        this._forwardPass = new ForwardPass(renderer, this, this._shadowPass);
        this._immediatePass = new ImmediatePass(renderer);
        this._skyDomePass = new SkyDomePass(renderer);
        this._ambientOcclusionPass = new AmbientOcclusionPass(renderer);
    }

    /// <summary>
    /// Gets the resource layout for transform data.
    /// </summary>
    internal ResourceLayout TransformResourceLayout => this._resources.TransformResourceLayout;

    /// <summary>
    /// Gets the resource layout for instance data.
    /// </summary>
    internal ResourceLayout InstanceResourceLayout => this._resources.InstanceResourceLayout;

    /// <summary>
    /// Gets the resource layout for skeleton data.
    /// </summary>
    internal ResourceLayout SkeletonResourceLayout => this._resources.SkeletonResourceLayout;

    /// <summary>
    /// Requests a data block for instance data.
    /// </summary>
    /// <param name="instanceDataBlockSize">The size of the instance data block.</param>
    internal DataBlock RequestInstanceDataBlock(int instanceDataBlockSize)
    {
        return this._resources.RequestInstanceDataBlock(instanceDataBlockSize);
    }

    /// <summary>
    /// Requests a data block for transform data.
    /// </summary>
    internal DataBlock RequestTransformDataBlock()
    {
        return this._resources.RequestTransformDataBlock();
    }

    /// <summary>
    /// Requests a data block for skeleton data.
    /// </summary>
    internal DataBlock RequestSkeletonDataBlock()
    {
        return this._resources.RequestSkeletonDataBlock();
    }

    /// <summary>
    /// Notifies the renderer that the given material's resources are dirty and need to be updated.
    /// </summary>
    /// <param name="material">The material to update.</param>
    internal void NotifyMaterialResourcesDirty(Material material)
    {
        this._resources.NotifyMaterialResourcesDirty(material);
    }

    /// <summary>
    /// Creates a new surface material of the specified type.
    /// </summary>
    /// <typeparam name="T">The material type, must have a <see cref="SurfaceShaderAttribute"/>.</typeparam>
    /// <returns>A new material instance.</returns>
    internal T MakeMaterial<T>() where T : Material, ICreateableMaterial<T>
    {
        var shaders = this.GetOrCreateShaders<T>();
        return T.Create(shaders);
    }

    /// <summary>
    /// Populates the internal shader cache for <typeparamref name="T"/> so subsequent material creation is allocation-only.
    /// </summary>
    /// <typeparam name="T">The material type, must have a <see cref="SurfaceShaderAttribute"/>.</typeparam>
    internal void PreloadMaterial<T>() where T : Material
    {
        this.GetOrCreateShaders<T>();
    }

    private ShaderSet GetOrCreateShaders<T>() where T : Material
    {
        var type = typeof(T);
        if (this._shaderCache.TryGetValue(type, out var cached))
        {
            return cached;
        }

        var attr = type.GetCustomAttribute<SurfaceShaderAttribute>()
            ?? throw new InvalidOperationException($"{type.Name} is missing the [SurfaceShader] attribute.");

        var textureNames = GetMaterialTextureNames(type);
        var hasParams = HasMaterialParams(type);
        var shaders = new ShaderSet
        {
            Forward = this.CreateShader(attr.FragmentPath, attr.VertexPath, textureNames, hasParams, ShaderPass.Forward, this._forwardPass),
            Shadow = this.CreateShader(attr.FragmentPath, attr.VertexPath, textureNames, hasParams, ShaderPass.Shadow, this._shadowPass),
            Picking = this.CreateShader(attr.FragmentPath, attr.VertexPath, textureNames, hasParams, ShaderPass.Picking, this._mousePickerPass)
        };

        this._shaderCache[type] = shaders;
        return shaders;
    }

    private static string[] GetMaterialTextureNames(Type materialType)
    {
        return materialType.GetProperties()
            .Select(p => (prop: p, attr: p.GetCustomAttribute<MaterialTextureAttribute>()))
            .Where(x => x.attr != null)
            .Select(x => x.attr!.Name ?? x.prop.Name)
            .ToArray();
    }

    private static bool HasMaterialParams(Type materialType)
    {
        var baseType = materialType.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Material<>))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    private Shader CreateShader(string? fragmentPath, string? vertexPath, string[] textureNames, bool hasParams, ShaderPass pass, IPipelineProvider pipelineProvider)
    {
        var vertexCode = ShaderLoader.AssembleVertexShader(vertexPath, pass);
        var fragmentCode = ShaderLoader.AssembleSurfaceShader(fragmentPath, pass);
        return new Shader(this._renderer, pipelineProvider, vertexCode, fragmentCode, textureNames, hasParams);
    }

    /// <summary>
    /// Updates the state of the 3D renderer before rendering.
    /// </summary>
    public void Update()
    {
        this._mousePickerPass.SetMousePosition(InputManager.Instance.CursorPosition);
    }

    /// <summary>
    /// Reads the picking result from the staging texture and updates the picking manager.
    /// Must be called after command submission.
    /// </summary>
    /// <param name="scene">The 3D scene whose picking manager should be updated, or null if no 3D scene is active.</param>
    public void ReadPickingResult(Scene3DLayer? scene)
    {
        if (scene == null) return;

        this._mousePickerPass.ReadStagingResult(scene.Picking);
    }

    /// <summary>
    /// Renders a <see cref="Scene3DLayer"/> to the specified <see cref="RenderTexture"/>.
    /// </summary>
    /// <param name="cl">The command list.</param>
    /// <param name="scene">The 3D scene to render.</param>
    /// <param name="renderTexture">The target render texture.</param>
    public void Render(CommandList cl, Scene3DLayer scene, RenderTexture renderTexture)
    {
        this._resources.Update(cl);

        var camera = scene.Camera;

        cl.SetFramebuffer(renderTexture.Framebuffer);
        this.ClearRenderTarget(cl, scene);

        if (camera == null) return;

        var opaqueRQ = scene.OpaqueRenderQueue;
        var transparentRQ = scene.TransparentRenderQueue;
        var immediateRQ = scene.ImmediateRenderables;
        var pickingRQ = scene.PickingRenderQueue;
        var shadowCasterRQs = new Span<RenderQueue>(scene.ShadowCasterRenderQueues, 0, scene.CascadesCount);

        var stats = this._renderer.Statistics;

        stats.ShadowSubPass.Begin();
        this._shadowPass.Render(cl, camera, scene.Environment.MainLight, shadowCasterRQs);
        stats.ShadowSubPass.End();

        stats.ForwardSubPass.Begin();
        this._forwardPass.Prepare(cl, renderTexture, camera, scene.Environment, opaqueRQ, transparentRQ);
        this._forwardPass.Draw(cl, opaqueRQ);
        stats.ForwardSubPass.End();

        stats.OtherSubPass.Begin();
        this._skyDomePass.Render(cl, renderTexture, camera, scene.Environment);
        if (scene.AmbientOcclusion.Enabled)
        {
            this._ambientOcclusionPass.Render(cl, renderTexture, camera, scene.AmbientOcclusion);
            cl.SetFramebuffer(renderTexture.Framebuffer);
        }
        stats.OtherSubPass.End();

        stats.ForwardSubPass.Begin();
        this._forwardPass.Draw(cl, transparentRQ);
        stats.ForwardSubPass.End();

        stats.OtherSubPass.Begin();
        this._immediatePass.Render(cl, renderTexture, camera, immediateRQ);
        this._particlesPass.Render(cl, renderTexture, camera, scene.ParticleSystems);
        stats.OtherSubPass.End();

        stats.PickingSubPass.Begin();
        this._mousePickerPass.Render(cl, renderTexture, camera, pickingRQ);
        stats.PickingSubPass.End();

        stats.OtherSubPass.Begin();
        this._gizmosPass.Render(cl, renderTexture, camera, scene.Gizmos);
        stats.OtherSubPass.End();
    }

    private void ClearRenderTarget(CommandList cl, Scene3DLayer scene)
    {
        ColorF? clearColor = scene.Camera?.ClearColor ?? scene.ClearColor;
        if (clearColor != null)
        {
            var col = clearColor.Value;
            cl.ClearColorTarget(0, new RgbaFloat(col.R, col.G, col.B, col.A));
            cl.ClearDepthStencil(1f);
        }
    }

    /// <summary>
    /// Disposes the 3D renderer and releases associated resources.
    /// </summary>
    public void Dispose()
    {
        this._mousePickerPass.Dispose();
        this._gizmosPass.Dispose();
        this._particlesPass.Dispose();
        this._shadowPass.Dispose();
        this._forwardPass.Dispose();
        this._skyDomePass.Dispose();
        this._immediatePass.Dispose();
        this._ambientOcclusionPass.Dispose();
        this._resources.Dispose();
        Instance = null!;
    }
}
