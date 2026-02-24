using System;
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
    }

    private readonly Kind _kind;
    private readonly float _value;

    private GuiSizeMode(Kind kind, float value)
    {
        this._kind = kind;
        this._value = value;
    }

    /// <summary>
    /// Gets a mode where the GUI renders at the native window resolution with no scaling.
    /// </summary>
    public static GuiSizeMode Native => new(Kind.Native, 0);

    /// <summary>
    /// Creates a mode where the GUI height is fixed and the width is derived from the window's aspect ratio.
    /// </summary>
    /// <param name="height">The desired GUI height in pixels.</param>
    /// <returns>A <see cref="GuiSizeMode"/> with a fixed height.</returns>
    public static GuiSizeMode FixedHeight(float height) => new(Kind.FixedHeight, height);

    /// <summary>
    /// Creates a mode where the GUI width is fixed and the height is derived from the window's aspect ratio.
    /// </summary>
    /// <param name="width">The desired GUI width in pixels.</param>
    /// <returns>A <see cref="GuiSizeMode"/> with a fixed width.</returns>
    public static GuiSizeMode FixedWidth(float width) => new(Kind.FixedWidth, width);

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
            Kind.FixedHeight => new Vector2(MathF.Round(this._value * aspectRatio), this._value),
            Kind.FixedWidth => new Vector2(this._value, MathF.Round(this._value / aspectRatio)),
            _ => windowSize,
        };
    }
}
