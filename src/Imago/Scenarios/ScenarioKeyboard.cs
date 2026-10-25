using System.Threading.Tasks;
using NeoVeldrid;

namespace Imago.Scenarios;

/// <summary>
/// The virtual keyboard of a <see cref="Scenario"/>. It goes through the same input path as a player's keyboard,
/// so key bindings and input blocking all apply.
/// </summary>
/// <remarks>
/// Each awaited action finishes after the game has seen it.
/// </remarks>
public sealed class ScenarioKeyboard
{
    private readonly Scenario _scenario;

    internal ScenarioKeyboard(Scenario scenario)
    {
        this._scenario = scenario;
    }

    /// <summary>
    /// Presses and releases a key.
    /// </summary>
    /// <param name="key">The key to press.</param>
    /// <returns>A task that completes once the key has been released.</returns>
    public async Task Press(Key key)
    {
        await this.Down(key);
        await this.Up(key);
    }

    /// <summary>
    /// Presses a key and keeps it held until <see cref="Up"/> is called.
    /// </summary>
    /// <param name="key">The key to press.</param>
    /// <returns>A task that completes once the game has seen the press.</returns>
    public Task Down(Key key)
    {
        this._scenario.App.Input.QueueKeyDown(key);
        return this._scenario.NextFrame();
    }

    /// <summary>
    /// Releases a held key.
    /// </summary>
    /// <param name="key">The key to release.</param>
    /// <returns>A task that completes once the game has seen the release.</returns>
    public Task Up(Key key)
    {
        this._scenario.App.Input.QueueKeyUp(key);
        return this._scenario.NextFrame();
    }

    /// <summary>
    /// Types the given text into the focused text box.
    /// </summary>
    /// <remarks>
    /// Only the typed characters are sent, not the key presses behind them.
    /// </remarks>
    /// <param name="text">The text to type.</param>
    /// <returns>A task that completes once the game has seen the text.</returns>
    public Task Type(string text)
    {
        this._scenario.App.Input.QueueText(text);
        return this._scenario.NextFrame();
    }
}
