using System.Numerics;

namespace Imago;

/// <summary>
/// Defines how the GUI resolution is determined relative to the window size.
/// </summary>
public readonly struct GuiSizeMode
{
    private enum Kind
    {
        Native,
        FixedHeight,
        FixedWidth,
        FitBox,
    }

    private readonly Kind _kind;
    private readonly float _width;
    private readonly float _height;

    private GuiSizeMode(Kind kind, float width, float height)
    {
        this._kind = kind;
        this._width = width;
        this._height = height;
    }

    /// <summary>
    /// Gets a mode where the GUI renders at the native window resolution with no scaling.
    /// </summary>
    public static GuiSizeMode Native => new(Kind.Native, 0, 0);

    /// <summary>
    /// Creates a mode where the GUI height is fixed and the width is derived from the window's aspect ratio.
    /// </summary>
    /// <param name="height">The desired GUI height in pixels.</param>
    /// <returns>A <see cref="GuiSizeMode"/> with a fixed height.</returns>
    public static GuiSizeMode FixedHeight(float height) => new(Kind.FixedHeight, 0, height);

    /// <summary>
    /// Creates a mode where the GUI width is fixed and the height is derived from the window's aspect ratio.
    /// </summary>
    /// <param name="width">The desired GUI width in pixels.</param>
    /// <returns>A <see cref="GuiSizeMode"/> with a fixed width.</returns>
    public static GuiSizeMode FixedWidth(float width) => new(Kind.FixedWidth, width, 0);

    /// <summary>
    /// Creates a mode where the given box always fits in the GUI. The GUI never gets smaller than the box,
    /// and the space the window has left over is added to the axis that does not limit the scale.
    /// </summary>
    /// <param name="width">The smallest GUI width in pixels.</param>
    /// <param name="height">The smallest GUI height in pixels.</param>
    /// <returns>A <see cref="GuiSizeMode"/> that fits the given box.</returns>
    public static GuiSizeMode FitBox(float width, float height) => new(Kind.FitBox, width, height);

    /// <summary>
    /// Computes the GUI size for the given window size.
    /// </summary>
    /// <param name="windowSize">The current window size in pixels.</param>
    /// <returns>The computed GUI size in pixels.</returns>
    public Vector2 ComputeSize(Vector2 windowSize)
    {
        float aspectRatio = windowSize.X / windowSize.Y;

        return this._kind switch
        {
            Kind.FixedHeight => new Vector2(float.Round(this._height * aspectRatio), this._height),
            Kind.FixedWidth => new Vector2(this._width, float.Round(this._width / aspectRatio)),
            Kind.FitBox => this.ComputeFitBoxSize(windowSize),
            _ => windowSize,
        };
    }

    private Vector2 ComputeFitBoxSize(Vector2 windowSize)
    {
        float scale = float.Min(windowSize.X / this._width, windowSize.Y / this._height);

        return new Vector2(float.Round(windowSize.X / scale), float.Round(windowSize.Y / scale));
    }
}
