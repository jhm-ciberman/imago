using System;

namespace LifeSim.Imago.Graphics;

/// <summary>
/// Event arguments for when the viewport is resized.
/// </summary>
public class ViewportResizedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportResizedEventArgs"/> class.
    /// </summary>
    /// <param name="width">The new width of the viewport.</param>
    /// <param name="height">The new height of the viewport.</param>
    public ViewportResizedEventArgs(uint width, uint height)
    {
        this.Width = width;
        this.Height = height;
    }

    /// <summary>
    /// Gets the new width of the viewport.
    /// </summary>
    public uint Width { get; }

    /// <summary>
    /// Gets the new height of the viewport.
    /// </summary>
    public uint Height { get; }
}
