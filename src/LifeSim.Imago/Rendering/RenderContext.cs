using System;
using LifeSim.Imago.Materials;
using LifeSim.Imago.Rendering.Shadows;
using LifeSim.Imago.SceneGraph;
using LifeSim.Imago.Textures;
using LifeSim.Support.Drawing;
using Veldrid;

namespace LifeSim.Imago.Rendering;

public class RenderContext : IDisposable
{
    private readonly ParticlesPass _particlesPass;
    private readonly SkyDomePass _skyDomePass;
    private readonly GizmosPass _gizmosPass;
    private readonly ForwardPass _forwardPass;
    private readonly ShadowPass _shadowPass;
    private readonly MousePickingPass _mousePickerPass;
    private readonly ImmediatePass _immediatePass;

    public RenderContext(Renderer renderer)
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
    /// Makes a new material.
    /// </summary>
    /// <returns>The created material.</returns>
    public Material MakeMaterial()
    {
        return new Material(this._forwardPass.DefaultShader, this._shadowPass.DefaultShader, this._mousePickerPass.DefaultShader);
    }

    /// <summary>
    /// Updates the renderer.
    /// </summary>
    /// <param name="inputSnapshot">The current input state.</param>
    public void Update(InputSnapshot inputSnapshot)
    {
        this._mousePickerPass.SetMousePosition(inputSnapshot.MousePosition);
    }

    public void Render(CommandList cl, Stage stage, RenderTexture renderTexture)
    {
        var scene = stage.Scene;
        var camera = scene.Camera;

        cl.SetFramebuffer(renderTexture.Framebuffer);
        ClearRenderTarget(cl, scene);

        if (camera == null) return;

        var opaqueRQ = stage.OpaqueRenderQueue;
        var transparentRQ = stage.TransparentRenderQueue;
        var immediateRQ = stage.ImmediateRenderables;
        var pickingRQ = stage.PickingRenderQueue;
        var shadowCasterRQs = new Span<RenderQueue>(stage.ShadowCasterRenderQueues, 0, stage.CascadesCount);

        this._shadowPass.Render(cl, camera, scene.Environment.MainLight, shadowCasterRQs);
        this._forwardPass.Render(cl, renderTexture, camera, scene.Environment, opaqueRQ, transparentRQ);
        this._immediatePass.Render(cl, renderTexture, camera, immediateRQ);
        this._skyDomePass.Render(cl, renderTexture, camera, scene.Environment);
        this._mousePickerPass.Render(cl, renderTexture, camera, stage.Picking, pickingRQ);
        this._particlesPass.Render(cl, renderTexture, camera, scene.ParticleSystems);
        this._gizmosPass.Render(cl, renderTexture, camera, stage.Gizmos);
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
