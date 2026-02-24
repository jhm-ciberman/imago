namespace Imago.Controls;

/// <summary>
/// Represents tooltip data that can be displayed for a control.
/// </summary>
public class Tooltip
{
    /// <summary>
    /// Gets or sets the default style that will be applied to tooltips when no specific style is set.
    /// </summary>
    public static IStyle? DefaultStyle { get; set; } = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tooltip"/> class.
    /// </summary>
    public Tooltip()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tooltip"/> class with the specified text and placement.
    /// </summary>
    /// <param name="text">The text content of the tooltip.</param>
    /// <param name="placement">The placement of the tooltip relative to its target control.</param>
    public Tooltip(string text, TooltipPlacement placement = TooltipPlacement.Top)
    {
        this.Text = text;
        this.Placement = placement;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tooltip"/> class with the specified content and placement.
    /// </summary>
    /// <param name="content">The custom content of the tooltip.</param>
    /// <param name="placement">The placement of the tooltip relative to its target control.</param>
    public Tooltip(Control content, TooltipPlacement placement = TooltipPlacement.Top)
    {
        this.Content = content;
        this.Placement = placement;
    }

    /// <summary>
    /// Gets or sets the text content of the tooltip.
    /// </summary>
    public string? Text { get; set; } = null;

    /// <summary>
    /// Gets or sets the custom content of the tooltip.
    /// </summary>
    public Control? Content { get; set; } = null;

    /// <summary>
    /// Gets or sets the placement of the tooltip relative to its target control.
    /// </summary>
    public TooltipPlacement Placement { get; set; } = TooltipPlacement.Top;

    /// <summary>
    /// Gets or sets the style of the tooltip.
    /// </summary>
    public IStyle? Style { get; set; } = null;
}
