using System;
using LifeSim.Imago.Assets.Materials;
using LifeSim.Imago.SceneGraph;
using LifeSim.Imago.Assets.Textures;
using LifeSim.Support.Drawing;
using Veldrid;
using LifeSim.Imago.Rendering.Passes.Shadows;
using LifeSim.Imago.Rendering.Passes;

namespace LifeSim.Imago.Rendering;

/// <summary>
/// Orchestrates the rendering of a <see cref="Layer3D"/> by coordinating various rendering passes.
/// </summary>
/// <remarks>
/// This class is responsible for setting up and executing the rendering pipeline for a 3D layer,
/// including shadow mapping, forward rendering, skybox, particles, and gizmos.
/// </remarks>
public class Forward3DRenderer : IDisposable
{
    private readonly ParticlesPass _particlesPass;
    private readonly SkyDomePass _skyDomePass;
    private readonly GizmosPass _gizmosPass;
    private readonly ForwardPass _forwardPass;
    private readonly ShadowPass _shadowPass;
    private readonly MousePickingPass _mousePickerPass;
    private readonly ImmediatePass _immediatePass;

    /// <summary>
    /// Initializes a new instance of the <see cref="Forward3DRenderer"/> class.
    /// </summary>
    /// <param name="renderer">The main renderer instance.</param>
    public Forward3DRenderer(Renderer renderer)
    {
        this._mousePickerPass = new MousePickingPass(renderer);
        this._gizmosPass = new GizmosPass(renderer);
        this._particlesPass = new ParticlesPass(renderer);
        this._shadowPass = new ShadowPass(renderer);
        this._forwardPass = new ForwardPass(renderer, this._shadowPass);
        this._immediatePass = new ImmediatePass(renderer);
        this._skyDomePass = new SkyDomePass(renderer);
    }

    /// <summary>
    /// Creates a new <see cref="Material"/> instance with default shaders.
    /// </summary>
    /// <returns>A new <see cref="Material"/> instance.</returns>
    public Material MakeMaterial()
    {
        return new Material(this._forwardPass.DefaultShader, this._shadowPass.DefaultShader, this._mousePickerPass.DefaultShader);
    }

    /// <summary>
    /// Updates the state of the 3D renderer before rendering.
    /// </summary>
    /// <param name="inputSnapshot">The current input snapshot.</param>
    public void Update(InputSnapshot inputSnapshot)
    {
        this._mousePickerPass.SetMousePosition(inputSnapshot.MousePosition);
    }

    /// <summary>
    /// Renders a <see cref="Layer3D"/> to the specified <see cref="RenderTexture"/>.
    /// </summary>
    /// <param name="cl">The Veldrid command list.</param>
    /// <param name="layer">The 3D layer to render.</param>
    /// <param name="renderTexture">The target render texture.</param>
    public void Render(CommandList cl, Layer3D layer, RenderTexture renderTexture)
    {
        var camera = layer.Camera;

        cl.SetFramebuffer(renderTexture.Framebuffer);
        this.ClearRenderTarget(cl, layer);

        if (camera == null) return;

        var opaqueRQ = layer.OpaqueRenderQueue;
        var transparentRQ = layer.TransparentRenderQueue;
        var immediateRQ = layer.ImmediateRenderables;
        var pickingRQ = layer.PickingRenderQueue;
        var shadowCasterRQs = new Span<RenderQueue>(layer.ShadowCasterRenderQueues, 0, layer.CascadesCount);

        this._shadowPass.Render(cl, camera, layer.Environment.MainLight, shadowCasterRQs);
        this._forwardPass.Render(cl, renderTexture, camera, layer.Environment, opaqueRQ, transparentRQ);
        this._skyDomePass.Render(cl, renderTexture, camera, layer.Environment);
        this._immediatePass.Render(cl, renderTexture, camera, immediateRQ);
        this._mousePickerPass.Render(cl, renderTexture, camera, layer.Picking, pickingRQ);
        this._particlesPass.Render(cl, renderTexture, camera, layer.ParticleSystems);
        this._gizmosPass.Render(cl, renderTexture, camera, layer.Gizmos);
    }

    private void ClearRenderTarget(CommandList cl, Layer3D layer)
    {
        ColorF? clearColor = layer.Camera?.ClearColor ?? layer.ClearColor;
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
