using System.Numerics;
using System.Threading.Tasks;
using NeoVeldrid;

namespace Imago.ScenarioRunner;

/// <summary>
/// The virtual mouse of a <see cref="Scenario"/>. It goes through the same input path as a player's mouse, so
/// hover, GUI hit testing, and input blocking all apply.
/// </summary>
/// <remarks>
/// Positions are in window pixels. Each awaited action finishes after the game has seen it.
/// </remarks>
public sealed class ScenarioMouse
{
    private const int DragSteps = 10;

    /// <summary>
    /// The frames to wait after a move before pressing or releasing. After one frame, the game can still act on
    /// what was under the old position.
    /// </summary>
    private const int HoverFrames = 2;

    private readonly Scenario _scenario;

    internal ScenarioMouse(Scenario scenario)
    {
        this._scenario = scenario;
    }

    /// <summary>
    /// Gets the position of the mouse, in window pixels.
    /// </summary>
    public Vector2 Position => this._scenario.App.Input.CursorPosition;

    /// <summary>
    /// Moves the mouse to the given position.
    /// </summary>
    /// <param name="position">The new position, in window pixels.</param>
    public void Move(Vector2 position)
    {
        this._scenario.App.Input.SetCursorPosition(position);
    }

    /// <summary>
    /// Moves the mouse to the given position and clicks there.
    /// </summary>
    /// <param name="position">The position to click, in window pixels.</param>
    /// <param name="button">The button to click with.</param>
    /// <returns>A task that completes once the button has been released.</returns>
    public async Task Click(Vector2 position, MouseButton button = MouseButton.Left)
    {
        this.Move(position);
        await this._scenario.Wait(HoverFrames);
        await this.Down(button);
        await this.Up(button);
    }

    /// <summary>
    /// Presses a mouse button at the current position.
    /// </summary>
    /// <param name="button">The button to press.</param>
    /// <returns>A task that completes once the game has seen the press.</returns>
    public Task Down(MouseButton button = MouseButton.Left)
    {
        this._scenario.App.Input.QueueMouseButtonDown(button);
        return this._scenario.NextFrame();
    }

    /// <summary>
    /// Releases a mouse button at the current position.
    /// </summary>
    /// <param name="button">The button to release.</param>
    /// <returns>A task that completes once the game has seen the release.</returns>
    public Task Up(MouseButton button = MouseButton.Left)
    {
        this._scenario.App.Input.QueueMouseButtonUp(button);
        return this._scenario.NextFrame();
    }

    /// <summary>
    /// Presses a mouse button at one position, moves to another over a few frames, and releases it there.
    /// </summary>
    /// <param name="from">The position to press at, in window pixels.</param>
    /// <param name="to">The position to release at, in window pixels.</param>
    /// <param name="button">The button to drag with.</param>
    /// <returns>A task that completes once the button has been released.</returns>
    public async Task Drag(Vector2 from, Vector2 to, MouseButton button = MouseButton.Left)
    {
        this.Move(from);
        await this._scenario.Wait(HoverFrames);
        await this.Down(button);

        for (int i = 1; i <= DragSteps; i++)
        {
            this.Move(Vector2.Lerp(from, to, (float)i / DragSteps));
            await this._scenario.NextFrame();
        }

        await this._scenario.Wait(HoverFrames);
        await this.Up(button);
    }

    /// <summary>
    /// Scrolls the mouse wheel at the current position.
    /// </summary>
    /// <param name="delta">The amount and direction to scroll, in the same units as the real mouse wheel.</param>
    /// <returns>A task that completes once the game has seen the scroll.</returns>
    public Task Scroll(float delta)
    {
        this._scenario.App.Input.QueueMouseWheel(delta);
        return this._scenario.NextFrame();
    }
}
