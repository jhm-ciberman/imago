using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Imago.Assets.Materials;
using Imago.Input;
using Imago.Assets.Textures;
using Imago.Rendering.Internals;
using Imago.Rendering.Passes;
using Imago.Rendering.Passes.Shadows;
using Imago.SceneGraph;
using Imago.Support.Drawing;
using Veldrid;
using Shader = Imago.Assets.Materials.Shader;

namespace Imago.Rendering;

/// <summary>
/// Orchestrates the rendering of a <see cref="Scene3D"/> by coordinating various rendering passes.
/// </summary>
/// <remarks>
/// This class is responsible for setting up and executing the rendering pipeline for a 3D layer,
/// including shadow mapping, forward rendering, skybox, particles, and gizmos.
/// </remarks>
public class Forward3DRenderer : IDisposable
{
    private readonly Renderer _renderer;
    private readonly ParticlesPass _particlesPass;
    private readonly SkyDomePass _skyDomePass;
    private readonly GizmosPass _gizmosPass;
    private readonly ForwardPass _forwardPass;
    private readonly ShadowPass _shadowPass;
    private readonly MousePickingPass _mousePickerPass;
    private readonly ImmediatePass _immediatePass;
    private readonly Dictionary<Type, ShaderSet> _shaderCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Forward3DRenderer"/> class.
    /// </summary>
    /// <param name="renderer">The main renderer instance.</param>
    public Forward3DRenderer(Renderer renderer)
    {
        this._renderer = renderer;
        this._mousePickerPass = new MousePickingPass(renderer);
        this._gizmosPass = new GizmosPass(renderer);
        this._particlesPass = new ParticlesPass(renderer);
        this._shadowPass = new ShadowPass(renderer);
        this._forwardPass = new ForwardPass(renderer, this._shadowPass);
        this._immediatePass = new ImmediatePass(renderer);
        this._skyDomePass = new SkyDomePass(renderer);
    }

    /// <summary>
    /// Creates a new surface material of the specified type.
    /// </summary>
    /// <typeparam name="T">The material type, must have a <see cref="SurfaceShaderAttribute"/>.</typeparam>
    /// <returns>A new material instance.</returns>
    public T MakeMaterial<T>() where T : Material, ICreateableMaterial<T>
    {
        var shaders = this.GetOrCreateShaders<T>();
        return T.Create(shaders);
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
    public void ReadPickingResult(Scene3D? scene)
    {
        if (scene == null) return;

        this._mousePickerPass.ReadStagingResult(scene.Picking);
    }

    /// <summary>
    /// Renders a <see cref="Scene3D"/> to the specified <see cref="RenderTexture"/>.
    /// </summary>
    /// <param name="cl">The Veldrid command list.</param>
    /// <param name="scene">The 3D scene to render.</param>
    /// <param name="renderTexture">The target render texture.</param>
    public void Render(CommandList cl, Scene3D scene, RenderTexture renderTexture)
    {
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
        this._forwardPass.Render(cl, renderTexture, camera, scene.Environment, opaqueRQ, transparentRQ);
        stats.ForwardSubPass.End();

        stats.OtherSubPass.Begin();
        this._skyDomePass.Render(cl, renderTexture, camera, scene.Environment);
        this._immediatePass.Render(cl, renderTexture, camera, immediateRQ);
        stats.OtherSubPass.End();

        stats.PickingSubPass.Begin();
        this._mousePickerPass.Render(cl, renderTexture, camera, scene.Picking, pickingRQ);
        stats.PickingSubPass.End();

        stats.OtherSubPass.Begin();
        this._particlesPass.Render(cl, renderTexture, camera, scene.ParticleSystems);
        this._gizmosPass.Render(cl, renderTexture, camera, scene.Gizmos);
        stats.OtherSubPass.End();
    }

    private void ClearRenderTarget(CommandList cl, Scene3D scene)
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
    }
}
