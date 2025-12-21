using System;
using System.Numerics;
using LifeSim.Imago.Rendering.Sprites;
using LifeSim.Support.Numerics;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Service that manages tooltip display and positioning.
/// </summary>
public class TooltipService : IDisposable
{
    /// <summary>
    /// Gets the singleton instance of the tooltip service.
    /// </summary>
    public static TooltipService Instance { get; } = new TooltipService();

    private TooltipPresenter? _activeTooltipPresenter;
    private Control? _currentTooltipOwner;
    private Rect _currentControlBounds;

    /// <summary>
    /// Initializes a new instance of the <see cref="TooltipService"/> class.
    /// </summary>
    private TooltipService()
    {
    }

    /// <summary>
    /// Shows a tooltip for the specified control relative to the control's bounds.
    /// </summary>
    /// <param name="control">The control that owns the tooltip.</param>
    public void ShowTooltip(Control control)
    {
        if (control.Tooltip == null || control.Layer == null) return;

        // Hide existing tooltip if different control
        if (this._currentTooltipOwner != null && this._currentTooltipOwner != control)
        {
            this.HideTooltip(this._currentTooltipOwner);
        }

        // Create or reuse tooltip presenter
        if (this._activeTooltipPresenter == null)
        {
            this._activeTooltipPresenter = new TooltipPresenter();
            // Ensure tooltip presenter has access to the layer
            this._activeTooltipPresenter.OnAddedToLayer(control.Layer);
        }

        this._currentTooltipOwner = control;
        this._currentControlBounds = new Rect(control.Position, control.ActualSize);
        this._activeTooltipPresenter.Tooltip = control.Tooltip;

        // Position calculation will happen in Draw() method
        this._activeTooltipPresenter.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Hides the currently displayed tooltip.
    /// </summary>
    /// <param name="control">The control that owns the tooltip.</param>
    public void HideTooltip(Control control)
    {
        if (this._currentTooltipOwner != control) return;

        this._activeTooltipPresenter?.Hide();
        this._currentTooltipOwner = null;
    }

    /// <summary>
    /// Hides the currently displayed tooltip, if any.
    /// </summary>
    public void HideTooltip()
    {
        if (this._currentTooltipOwner != null)
        {
            this.HideTooltip(this._currentTooltipOwner);
        }
    }

    /// <summary>
    /// Updates the tooltip service.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    public void Update(float deltaTime)
    {
        this._activeTooltipPresenter?.Update(deltaTime);
    }

    /// <summary>
    /// Calculates the position for a tooltip based on its placement preference.
    /// </summary>
    /// <param name="placement">The desired placement of the tooltip.</param>
    /// <param name="controlBounds">The bounds of the control that owns the tooltip.</param>
    /// <param name="tooltipSize">The size of the tooltip.</param>
    /// <returns>The calculated position for the tooltip.</returns>
    private static Vector2 CalculateTooltipPosition(TooltipPlacement placement, Rect controlBounds, Vector2 tooltipSize)
    {
        const float margin = 5f;

        float x = placement switch
        {
            TooltipPlacement.Left or TooltipPlacement.LeftStart or TooltipPlacement.LeftEnd => controlBounds.X - tooltipSize.X - margin,
            TooltipPlacement.Right or TooltipPlacement.RightStart or TooltipPlacement.RightEnd => controlBounds.Right + margin,
            TooltipPlacement.TopStart or TooltipPlacement.BottomStart => controlBounds.X,
            TooltipPlacement.TopEnd or TooltipPlacement.BottomEnd => controlBounds.Right - tooltipSize.X,
            TooltipPlacement.Top or TooltipPlacement.Bottom => controlBounds.X + (controlBounds.Width - tooltipSize.X) / 2f,
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };

        float y = placement switch
        {
            TooltipPlacement.Top or TooltipPlacement.TopStart or TooltipPlacement.TopEnd => controlBounds.Y - tooltipSize.Y - margin,
            TooltipPlacement.Bottom or TooltipPlacement.BottomStart or TooltipPlacement.BottomEnd => controlBounds.Bottom + margin,
            TooltipPlacement.LeftStart or TooltipPlacement.RightStart => controlBounds.Y,
            TooltipPlacement.LeftEnd or TooltipPlacement.RightEnd => controlBounds.Bottom - tooltipSize.Y,
            TooltipPlacement.Left or TooltipPlacement.Right => controlBounds.Y + (controlBounds.Height - tooltipSize.Y) / 2f,
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };

        return new Vector2(x, y);
    }

    /// <summary>
    /// Draws all active tooltips.
    /// </summary>
    /// <param name="ctx">The drawing context to use.</param>
    public void Draw(DrawingContext ctx)
    {
        if (this._activeTooltipPresenter != null && this._currentTooltipOwner?.Layer != null && this._currentTooltipOwner.Tooltip != null)
        {
            // Ensure tooltip is properly measured and arranged before drawing
            var layer = this._currentTooltipOwner.Layer;
            var viewportSize = layer.Viewport.Size / layer.Zoom;

            // Measure to get desired size
            this._activeTooltipPresenter.Measure(viewportSize);
            var tooltipSize = this._activeTooltipPresenter.DesiredSize;

            // Calculate optimal position based on placement preference
            var placement = this._currentTooltipOwner.Tooltip.Placement;
            var optimalPosition = CalculateTooltipPosition(placement, this._currentControlBounds, tooltipSize);

            // Position tooltip at the calculated coordinates
            var tooltipRect = new Rect(optimalPosition, tooltipSize);
            this._activeTooltipPresenter.Arrange(tooltipRect);

            this._activeTooltipPresenter.Draw(ctx);
        }
    }

    /// <summary>
    /// Disposes the tooltip service and its resources.
    /// </summary>
    public void Dispose()
    {
        if (this._activeTooltipPresenter != null && this._currentTooltipOwner?.Layer != null)
        {
            this._activeTooltipPresenter.OnRemovedFromLayer(this._currentTooltipOwner.Layer);
        }
        this._activeTooltipPresenter?.Dispose();
        this._activeTooltipPresenter = null;
        this._currentTooltipOwner = null;
    }
}
