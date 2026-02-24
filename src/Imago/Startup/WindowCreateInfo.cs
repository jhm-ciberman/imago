using Veldrid;

namespace Imago.Startup;

/// <summary>
/// Specifies the properties used to create a new window.
/// </summary>
public struct WindowCreateInfo
{
    /// <summary>
    /// The initial X position of the window.
    /// </summary>
    public int X;

    /// <summary>
    /// The initial Y position of the window.
    /// </summary>
    public int Y;

    /// <summary>
    /// The initial width of the window in pixels.
    /// </summary>
    public int WindowWidth;

    /// <summary>
    /// The initial height of the window in pixels.
    /// </summary>
    public int WindowHeight;

    /// <summary>
    /// The initial state of the window (e.g., Normal, Maximized, FullScreen).
    /// </summary>
    public WindowState WindowInitialState;

    /// <summary>
    /// The initial title of the window.
    /// </summary>
    public string WindowTitle;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowCreateInfo"/> struct.
    /// </summary>
    /// <param name="x">The initial X position of the window.</param>
    /// <param name="y">The initial Y position of the window.</param>
    /// <param name="windowWidth">The initial width of the window in pixels.</param>
    /// <param name="windowHeight">The initial height of the window in pixels.</param>
    /// <param name="windowInitialState">The initial state of the window.</param>
    /// <param name="windowTitle">The initial title of the window.</param>
    public WindowCreateInfo(
        int x,
        int y,
        int windowWidth,
        int windowHeight,
        WindowState windowInitialState,
        string windowTitle)
    {
        X = x;
        Y = y;
        WindowWidth = windowWidth;
        WindowHeight = windowHeight;
        WindowInitialState = windowInitialState;
        WindowTitle = windowTitle;
    }
}
