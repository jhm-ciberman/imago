using System;

namespace Imago.ScenarioRunner;

/// <summary>
/// Overrides the window size for a single scenario, taking precedence over the size its host gives the window.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class WindowSizeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WindowSizeAttribute"/> class.
    /// </summary>
    /// <param name="width">The window width, in pixels.</param>
    /// <param name="height">The window height, in pixels.</param>
    public WindowSizeAttribute(int width, int height)
    {
        this.Width = width;
        this.Height = height;
    }

    /// <summary>
    /// Gets the window width, in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the window height, in pixels.
    /// </summary>
    public int Height { get; }
}
