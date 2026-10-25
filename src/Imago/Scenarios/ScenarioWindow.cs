using System.Numerics;
using System.Threading.Tasks;

namespace Imago.Scenarios;

/// <summary>
/// The window of the application a <see cref="Scenario"/> drives.
/// </summary>
public sealed class ScenarioWindow
{
    private readonly Scenario _scenario;

    internal ScenarioWindow(Scenario scenario)
    {
        this._scenario = scenario;
    }

    /// <summary>
    /// Gets the size of the window, in pixels.
    /// </summary>
    public Vector2 Size => this._scenario.App.Viewport.Size;

    /// <summary>
    /// Resizes the window.
    /// </summary>
    /// <param name="width">The new width, in pixels.</param>
    /// <param name="height">The new height, in pixels.</param>
    /// <returns>A task that completes once the game has been laid out at the new size.</returns>
    public async Task Resize(int width, int height)
    {
        var window = this._scenario.App.Window;
        window.Width = width;
        window.Height = height;

        var size = new Vector2(width, height);
        await this._scenario.WaitUntil(() => this.Size == size, $"the window to resize to {width}x{height}");

        // The GUI lays out at the new size on the next update.
        await this._scenario.NextFrame();
    }
}
