namespace LifeSim.Engine.Controls;

/// <summary>
/// Specifies the visibility of a control.
/// </summary>
public enum Visibility
{
    /// <summary>
    /// The control is visible and will be rendered normally.
    /// </summary>
    Visible = 0,

    /// <summary>
    /// The control is hidden. This means that it will not be rendered but the space it occupies will be preserved.
    /// </summary>
    Hidden = 1,

    /// <summary>
    /// The control is collapsed. This means that it will not be rendered and will not occupy any space in the layout.
    /// </summary>
    Collapsed = 2,
}
