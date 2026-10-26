using System;
using System.Collections.Generic;
using Imago.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Imago.Rendering;

/// <summary>
/// Captures a rendered <see cref="Stage"/> into an image.
/// </summary>
/// <remarks>
/// Blocks while the GPU copy completes, so this is meant for occasional captures such as screenshots,
/// not per-frame use. Hold an instance to reuse it across captures.
/// </remarks>
public sealed class StageCapturer : IDisposable
{
    private readonly Renderer _renderer;
    private readonly List<IRenderLayer> _hiddenLayers = new();
    private RenderTexture? _target;

    /// <summary>
    /// Initializes a new instance of the <see cref="StageCapturer"/> class.
    /// </summary>
    /// <param name="renderer">The renderer used to capture the stage.</param>
    public StageCapturer(Renderer renderer)
    {
        this._renderer = renderer;
    }

    /// <summary>
    /// Captures the given stage into a new image.
    /// </summary>
    /// <remarks>
    /// Honors scene and layer visibility. The returned image has a top-left origin and is owned by the caller.
    /// </remarks>
    /// <param name="stage">The stage to capture.</param>
    /// <param name="width">The capture width in pixels. Defaults to the current screen width.</param>
    /// <param name="height">The capture height in pixels. Defaults to the current screen height.</param>
    /// <param name="includeUi">Whether to keep the layers drawn above the main render target, like the GUI, or capture only the main layers.</param>
    /// <returns>A new image holding the captured frame.</returns>
    public Image<Rgba32> Capture(Stage stage, uint? width = null, uint? height = null, bool includeUi = true)
    {
        uint targetWidth = width ?? this._renderer.FullScreenRenderTexture.Width;
        uint targetHeight = height ?? this._renderer.FullScreenRenderTexture.Height;

        var target = this.GetTarget(targetWidth, targetHeight);

        if (!includeUi)
        {
            HideLayers(stage.GuiLayers, this._hiddenLayers);
            HideLayers(stage.OverlayLayers, this._hiddenLayers);
        }

        try
        {
            this._renderer.RenderStageOffscreen(stage, target);
        }
        finally
        {
            ShowLayers(this._hiddenLayers);
            this._hiddenLayers.Clear();
        }

        return this._renderer.ReadColorToImage(target);
    }

    private static void HideLayers(IReadOnlyList<IRenderLayer> layers, List<IRenderLayer> hiddenLayers)
    {
        foreach (var layer in layers)
        {
            if (layer.IsVisible)
            {
                layer.IsVisible = false;
                hiddenLayers.Add(layer);
            }
        }
    }

    private static void ShowLayers(IReadOnlyList<IRenderLayer> layers)
    {
        foreach (var layer in layers)
        {
            layer.IsVisible = true;
        }
    }

    private RenderTexture GetTarget(uint width, uint height)
    {
        if (this._target == null)
        {
            this._target = new RenderTexture(width, height);
        }
        else
        {
            this._target.Resize(width, height);
        }

        return this._target;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this._target?.Dispose();
    }
}
