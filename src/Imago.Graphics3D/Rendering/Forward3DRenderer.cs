using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Imago.Graphics3D.Materials;
using Imago.Graphics3D.Metrics;
using Imago.Graphics3D.Rendering.Internals;
using Imago.Graphics3D.Rendering.Internals.Buffers;
using Imago.Graphics3D.Rendering.Passes;
using Imago.Graphics3D.Rendering.Passes.Shadows;
using Imago.Graphics3D.SceneGraph;
using Imago.Graphics3D.SceneGraph.Picking;
using Imago.Metrics;
using Imago.Rendering;
using Imago.Support.Drawing;
using Imago.Textures;
using NeoVeldrid;
using Shader = Imago.Graphics3D.Materials.Shader;

namespace Imago.Graphics3D.Rendering;

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

    internal Graphics3DMetrics Metrics { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Forward3DRenderer"/> class.
    /// </summary>
    /// <param name="renderer">The main renderer instance.</param>
    /// <param name="metrics">The metrics of the application. It must have the <see cref="Graphics3DMetrics"/> group.</param>
    /// <exception cref="InvalidOperationException">Thrown if a 3D renderer already exists.</exception>
    public Forward3DRenderer(Renderer renderer, FrameMetrics metrics)
    {
        if (Instance != null)
        {
            throw new InvalidOperationException("Only one instance of Forward3DRenderer can be created.");
        }

        Instance = this;

        this._renderer = renderer;
        this.Metrics = metrics.Graphics3D;
        this._resources = new RendererResources(renderer.GraphicsDevice);
        this._mousePickerPass = new MousePickingPass(renderer, this);
        this._gizmosPass = new GizmosPass(renderer);
        this._particlesPass = new ParticlesPass(renderer, this.Metrics.ParticleCount);
        this._shadowPass = new ShadowPass(renderer, this);
        this._forwardPass = new ForwardPass(renderer, this, this._shadowPass);
        this._immediatePass = new ImmediatePass(renderer);
        this._skyDomePass = new SkyDomePass(renderer);
        this._ambientOcclusionPass = new AmbientOcclusionPass(renderer);

        renderer.FrameCompleted += this.Renderer_FrameCompleted;
    }

    /// <summary>
    /// Updates the given picking manager with the object under the cursor, as read from an earlier frame.
    /// Does nothing if no new result was read since the last call.
    /// </summary>
    /// <param name="pickingManager">The picking manager to update.</param>
    internal void ApplyPickingResult(PickingManager pickingManager)
    {
        this._mousePickerPass.ApplyResult(pickingManager);
    }

    private void Renderer_FrameCompleted(object? sender, EventArgs e)
    {
        this._mousePickerPass.ReadStagingResult();
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
    internal T MakeMaterial<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>() where T : Material, ICreateableMaterial<T>
    {
        var shaders = this.GetOrCreateShaders<T>();
        return T.Create(shaders);
    }

    /// <summary>
    /// Populates the internal shader cache for <typeparamref name="T"/> so subsequent material creation is allocation-only.
    /// </summary>
    /// <typeparam name="T">The material type, must have a <see cref="SurfaceShaderAttribute"/>.</typeparam>
    internal void PreloadMaterial<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>() where T : Material
    {
        this.GetOrCreateShaders<T>();
    }

    private ShaderSet GetOrCreateShaders<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>() where T : Material
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

    private static string[] GetMaterialTextureNames([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type materialType)
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
    /// Renders a <see cref="Scene3DLayer"/> to the specified <see cref="RenderTexture"/>.
    /// </summary>
    /// <param name="cl">The command list.</param>
    /// <param name="scene">The 3D scene to render.</param>
    /// <param name="renderTexture">The target render texture.</param>
    public void Render(CommandList cl, Scene3DLayer scene, RenderTexture renderTexture)
    {
        var metrics = this.Metrics;

        using (metrics.BufferUploadTime.StartTimer())
        {
            this._resources.Update(cl);
        }

        var camera = scene.Camera;

        cl.SetFramebuffer(renderTexture.Framebuffer);
        this.ClearRenderTarget(cl, scene);

        if (camera == null) return;

        var opaqueRQ = scene.OpaqueRenderQueue;
        var transparentRQ = scene.TransparentRenderQueue;
        var immediateRQ = scene.ImmediateRenderables;
        var pickingRQ = scene.PickingRenderQueue;
        var shadowCasterRQs = new Span<RenderQueue>(scene.ShadowCasterRenderQueues, 0, scene.CascadesCount);

        metrics.ShadowCascadeCount.Set(scene.CascadesCount);

        using (metrics.ShadowPassTime.StartTimer())
        {
            this._shadowPass.Render(cl, camera, scene.Environment.MainLight, shadowCasterRQs);
        }

        using (metrics.ForwardPassTime.StartTimer())
        {
            this._forwardPass.Prepare(cl, renderTexture, camera, scene.Environment, opaqueRQ, transparentRQ);
            this._forwardPass.Draw(cl, opaqueRQ);
        }

        using (metrics.SkyDomePassTime.StartTimer())
        {
            this._skyDomePass.Render(cl, renderTexture, camera, scene.Environment);
        }

        if (scene.AmbientOcclusion.Enabled)
        {
            using (metrics.AmbientOcclusionPassTime.StartTimer())
            {
                this._ambientOcclusionPass.Render(cl, renderTexture, camera, scene.AmbientOcclusion);
                cl.SetFramebuffer(renderTexture.Framebuffer);
            }
        }

        using (metrics.ForwardPassTime.StartTimer())
        {
            this._forwardPass.Draw(cl, transparentRQ);
        }

        using (metrics.ImmediatePassTime.StartTimer())
        {
            this._immediatePass.Render(cl, renderTexture, camera, immediateRQ);
        }

        using (metrics.ParticlesPassTime.StartTimer())
        {
            this._particlesPass.Render(cl, renderTexture, camera, scene.ParticleSystems);
        }

        using (metrics.GizmosPassTime.StartTimer())
        {
            this._gizmosPass.Render(cl, renderTexture, camera, scene.Gizmos);
        }

        using (metrics.PickingPassTime.StartTimer())
        {
            this._mousePickerPass.Render(cl, renderTexture, camera, pickingRQ);
        }
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
        this._renderer.FrameCompleted -= this.Renderer_FrameCompleted;
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
