namespace Imago.Controls;

/// <summary>
/// Specifies the placement of a tooltip relative to its target control.
/// </summary>
public enum TooltipPlacement
{
    /// <summary>
    /// The tooltip appears above the control, centered horizontally.
    /// </summary>
    Top = 0,

    /// <summary>
    /// The tooltip appears above the control, aligned to the left edge.
    /// </summary>
    TopStart,

    /// <summary>
    /// The tooltip appears above the control, aligned to the right edge.
    /// </summary>
    TopEnd,

    /// <summary>
    /// The tooltip appears below the control, centered horizontally.
    /// </summary>
    Bottom,

    /// <summary>
    /// The tooltip appears below the control, aligned to the left edge.
    /// </summary>
    BottomStart,

    /// <summary>
    /// The tooltip appears below the control, aligned to the right edge.
    /// </summary>
    BottomEnd,

    /// <summary>
    /// The tooltip appears to the left of the control, centered vertically.
    /// </summary>
    Left,

    /// <summary>
    /// The tooltip appears to the left of the control, aligned to the top edge.
    /// </summary>
    LeftStart,

    /// <summary>
    /// The tooltip appears to the left of the control, aligned to the bottom edge.
    /// </summary>
    LeftEnd,

    /// <summary>
    /// The tooltip appears to the right of the control, centered vertically.
    /// </summary>
    Right,

    /// <summary>
    /// The tooltip appears to the right of the control, aligned to the top edge.
    /// </summary>
    RightStart,

    /// <summary>
    /// The tooltip appears to the right of the control, aligned to the bottom edge.
    /// </summary>
    RightEnd,
}
