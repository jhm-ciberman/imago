using System;

namespace Imago.SceneGraph;

/// <summary>
/// Event arguments for the <see cref="Stage.ScreenChanged"/> event.
/// </summary>
public class ScreenChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the old screen.
    /// </summary>
    public Screen? OldScreen { get; }

    /// <summary>
    /// Gets the new screen.
    /// </summary>
    public Screen? NewScreen { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenChangedEventArgs"/> class.
    /// </summary>
    /// <param name="oldScreen">The old screen.</param>
    /// <param name="newScreen">The new screen.</param>
    public ScreenChangedEventArgs(Screen oldScreen, Screen newScreen)
    {
        this.OldScreen = oldScreen;
        this.NewScreen = newScreen;
    }
}
