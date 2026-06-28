using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Imago.Input;
using Imago.SceneGraph;
using Imago.Support;
using SixLabors.ImageSharp;

namespace Imago.ScenarioRunner;

/// <summary>
/// Base class for scenario classes. Methods marked with <see cref="ScenarioAttribute"/> use these inherited
/// helpers to wait for frames, move the cursor, and capture screenshots.
/// </summary>
/// <remarks>
/// This base knows only about the engine. A game typically derives its own base from this to add helpers that
/// navigate that game's screens and wait for its state.
/// </remarks>
public abstract class ScenarioBase
{
    private const double DefaultTimeoutSeconds = 20.0;

    private ScenarioContext _context = null!;
    private int _captureCount;

    /// <summary>
    /// Waits until the next frame has been rendered.
    /// </summary>
    /// <returns>A task that completes on the next frame.</returns>
    protected Task NextFrame()
    {
        return this._context.Scheduler.WaitFrames(1, DefaultTimeoutSeconds);
    }

    /// <summary>
    /// Waits the given number of frames, letting pending changes render before continuing.
    /// </summary>
    /// <param name="count">The number of frames to wait.</param>
    /// <returns>A task that completes once the frames have elapsed.</returns>
    protected Task Frames(int count)
    {
        return this._context.Scheduler.WaitFrames(count, DefaultTimeoutSeconds);
    }

    /// <summary>
    /// Waits until <paramref name="condition"/> holds, failing if it does not within the timeout.
    /// </summary>
    /// <param name="condition">The condition to poll once per frame.</param>
    /// <param name="because">A description of what is awaited, shown if the wait times out.</param>
    /// <param name="timeoutSeconds">The maximum time to wait, in seconds.</param>
    /// <returns>A task that completes when the condition holds.</returns>
    protected Task WaitUntil(Func<bool> condition, string because, double timeoutSeconds = DefaultTimeoutSeconds)
    {
        return this._context.Scheduler.WaitUntil(condition, because, timeoutSeconds);
    }

    /// <summary>
    /// Moves the virtual cursor to a screen position, in window pixels, so hover and picking follow it.
    /// </summary>
    /// <param name="screenPosition">The cursor position in window pixels.</param>
    protected void MoveCursor(Vector2 screenPosition)
    {
        InputManager.Instance.SetCursorPosition(screenPosition);
    }

    /// <summary>
    /// Writes a progress line to the console.
    /// </summary>
    /// <param name="message">The message to write.</param>
    protected void Log(string message)
    {
        Console.WriteLine($"  [scenario] {message}");
    }

    /// <summary>
    /// Saves a screenshot of the current frame to the scenario's output folder.
    /// </summary>
    /// <remarks>
    /// Waits <paramref name="settleFrames"/> frames first so the latest changes are rendered before the capture.
    /// Files are numbered in capture order.
    /// </remarks>
    /// <param name="label">A short label included in the file name, or null to use the scenario name.</param>
    /// <param name="includeUi">Whether to keep the GUI, cursor, and tooltips, or capture only the 3D world.</param>
    /// <param name="settleFrames">The number of frames to wait before capturing.</param>
    /// <returns>A task that completes once the screenshot is written.</returns>
    protected async Task Capture(string? label = null, bool includeUi = true, int settleFrames = 2)
    {
        await this.Frames(settleFrames);

        label ??= this._context.Name;

        var stage = this._context.App.Stage;
        var hiddenLayers = new List<ILayer2D>();
        if (!includeUi)
        {
            foreach (var layer in stage.Layers)
            {
                if (layer is ILayer2D layer2D && layer2D.IsVisible)
                {
                    layer2D.IsVisible = false;
                    hiddenLayers.Add(layer2D);
                }
            }
        }

        try
        {
            using var image = this._context.Capturer.Capture(stage);
            string fileName = $"{this._captureCount:D2}_{label.Slug()}.png";
            this._captureCount++;

            string path = Path.Combine(this._context.OutputDirectory, fileName);
            image.SaveAsPng(path);
            this.Log($"captured {fileName} ({image.Width}x{image.Height})");
        }
        finally
        {
            foreach (var layer in hiddenLayers)
            {
                layer.IsVisible = true;
            }
        }
    }

    internal void Bind(ScenarioContext context)
    {
        this._context = context;
    }
}
