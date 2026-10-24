using System;
using System.Numerics;
using Imago.Rendering;
using Imago.Rendering.Sprites;
using Imago.Support.Numerics;

namespace Imago.Gui;

/// <summary>
/// A dedicated layer for rendering tooltips above all other UI content.
/// </summary>
/// <remarks>
/// Controls show their tooltips in the tooltip layer of their stage. Without one, tooltips are not shown.
/// </remarks>
public class TooltipLayer : IRenderLayer, IDisposable
{
    private readonly TooltipPresenter _presenter;

    private Control? _currentOwner;
    private Rect _ownerBounds;

    /// <inheritdoc />
    public int ZOrder => 10000;

    /// <inheritdoc />
    public bool IsVisible { get; set; } = true;

    /// <inheritdoc />
    public bool IsInputBlocked { get; set; }

    /// <inheritdoc />
    public Stage? Stage { get; private set; }

    /// <inheritdoc />
    public event EventHandler? Mounted;

    /// <inheritdoc />
    public event EventHandler? Unmounting;

    /// <inheritdoc />
    public void Mount(Stage stage)
    {
        this.Stage = stage;
        this.Mounted?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void Unmount()
    {
        this.Unmounting?.Invoke(this, EventArgs.Empty);
        this.Stage = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TooltipLayer"/> class.
    /// </summary>
    public TooltipLayer()
    {
        this._presenter = new TooltipPresenter();
    }

    /// <summary>
    /// Shows a tooltip for the specified control.
    /// </summary>
    /// <param name="control">The control that owns the tooltip.</param>
    public void Show(Control control)
    {
        if (control.Tooltip == null || control.Layer == null)
        {
            return;
        }

        if (this._currentOwner != null && this._currentOwner != control)
        {
            this.Hide(this._currentOwner);
        }

        // Ensure presenter has access to a layer for resource loading and layout
        if (this._presenter.Layer == null)
        {
            this._presenter.Mount(control.Layer);
        }

        this._currentOwner = control;
        this._ownerBounds = new Rect(control.Position, control.ActualSize);
        this._presenter.Tooltip = control.Tooltip;
        this._presenter.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Hides the tooltip for the specified control.
    /// </summary>
    /// <param name="control">The control that owns the tooltip.</param>
    public void Hide(Control control)
    {
        if (this._currentOwner != control)
        {
            return;
        }

        this._presenter.Hide();
        this._currentOwner = null;
    }

    /// <summary>
    /// Hides the currently displayed tooltip, if any.
    /// </summary>
    public void Hide()
    {
        if (this._currentOwner != null)
        {
            this.Hide(this._currentOwner);
        }
    }

    /// <inheritdoc />
    public void Update(float deltaTime)
    {
        this._presenter.Update(deltaTime);
    }

    /// <inheritdoc />
    public void Render(RenderContext ctx)
    {
        this.Draw(ctx.SpriteBatch, new Vector2(ctx.Target.Width, ctx.Target.Height));
    }

    private void Draw(SpriteBatch batch, Vector2 targetSize)
    {
        if (this._currentOwner?.Layer == null || this._currentOwner.Tooltip == null)
        {
            return;
        }

        var ownerLayer = this._currentOwner.Layer;
        var viewportSize = ownerLayer.Viewport.Size / ownerLayer.Zoom;

        this._presenter.Measure(viewportSize);
        var tooltipSize = this._presenter.DesiredSize;

        var placement = this._currentOwner.Tooltip.Placement;
        var optimalPosition = CalculateTooltipPosition(placement, this._ownerBounds, tooltipSize);

        var tooltipRect = new Rect(optimalPosition, tooltipSize);
        this._presenter.Arrange(tooltipRect);

        batch.Begin(Matrix3x2.CreateScale(targetSize / viewportSize));
        this._presenter.Draw(batch);
        batch.End();
    }

    private static Vector2 CalculateTooltipPosition(TooltipPlacement placement, Rect controlBounds, Vector2 tooltipSize)
    {
        const float margin = 5f;

        float x = placement switch
        {
            TooltipPlacement.Left or TooltipPlacement.LeftStart or TooltipPlacement.LeftEnd
                => controlBounds.X - tooltipSize.X - margin,
            TooltipPlacement.Right or TooltipPlacement.RightStart or TooltipPlacement.RightEnd
                => controlBounds.Right + margin,
            TooltipPlacement.TopStart or TooltipPlacement.BottomStart
                => controlBounds.X,
            TooltipPlacement.TopEnd or TooltipPlacement.BottomEnd
                => controlBounds.Right - tooltipSize.X,
            TooltipPlacement.Top or TooltipPlacement.Bottom
                => controlBounds.X + (controlBounds.Width - tooltipSize.X) / 2f,
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };

        float y = placement switch
        {
            TooltipPlacement.Top or TooltipPlacement.TopStart or TooltipPlacement.TopEnd
                => controlBounds.Y - tooltipSize.Y - margin,
            TooltipPlacement.Bottom or TooltipPlacement.BottomStart or TooltipPlacement.BottomEnd
                => controlBounds.Bottom + margin,
            TooltipPlacement.LeftStart or TooltipPlacement.RightStart
                => controlBounds.Y,
            TooltipPlacement.LeftEnd or TooltipPlacement.RightEnd
                => controlBounds.Bottom - tooltipSize.Y,
            TooltipPlacement.Left or TooltipPlacement.Right
                => controlBounds.Y + (controlBounds.Height - tooltipSize.Y) / 2f,
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };

        return new Vector2(x, y);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (this._presenter.Layer != null)
        {
            this._presenter.Unmount();
        }

        this._presenter.Dispose();
    }
}
