using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Imago.Controls.Drawing;
using LifeSim.Imago.Input;
using LifeSim.Imago.Rendering.Sprites;
using LifeSim.Support.Numerics;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Represents the base class for all UI elements that have a visual representation in the 2D GUI.
/// </summary>
public class Control : Visual
{
    /// <summary>
    /// Occurs when the mouse pointer enters the bounds of the control.
    /// </summary>
    public EventHandler? MouseEnter;

    /// <summary>
    /// Occurs when the mouse pointer leaves the bounds of the control.
    /// </summary>
    public EventHandler? MouseLeave;

    /// <summary>
    /// Occurs when a mouse button is pressed while the pointer is over the control.
    /// </summary>
    public EventHandler<MouseButtonEventArgs>? MouseDown;

    /// <summary>
    /// Occurs when a mouse button is released while the pointer is over the control.
    /// </summary>
    public EventHandler<MouseButtonEventArgs>? MouseUp;

    /// <summary>
    /// Occurs when the mouse wheel is scrolled while the pointer is over the control.
    /// </summary>
    public EventHandler<MouseWheelEventArgs>? MouseWheel;

    private Thickness _margin = new Thickness(0);
    private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Left;
    private VerticalAlignment _verticalAlignment = VerticalAlignment.Top;
    private Dock _dock = Dock.Left;
    private IBackground? _background = null;
    private float _width = float.NaN;
    private float _height = float.NaN;

    private float _left = float.NaN;
    private float _top = float.NaN;
    private float _right = float.NaN;
    private float _bottom = float.NaN;
    private Tooltip? _tooltip = null;

    /// <summary>
    /// Gets or sets the margin of the control, which is the space around the control.
    /// </summary>
    public Thickness Margin
    {
        get => this._margin;
        set => this.SetPropertyAndInvalidateMeasure(ref this._margin, value);
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of the control within its parent's layout slot.
    /// </summary>
    public HorizontalAlignment HorizontalAlignment
    {
        get => this._horizontalAlignment;
        set => this.SetPropertyAndInvalidateMeasure(ref this._horizontalAlignment, value);
    }

    /// <summary>
    /// Gets or sets the vertical alignment of the control within its parent's layout slot.
    /// </summary>
    public VerticalAlignment VerticalAlignment
    {
        get => this._verticalAlignment;
        set => this.SetPropertyAndInvalidateMeasure(ref this._verticalAlignment, value);
    }

    /// <summary>
    /// Gets or sets the dock position of the control within a <see cref="DockPanel"/>.
    /// </summary>
    public Dock Dock
    {
        get => this._dock;
        set => this.SetPropertyAndInvalidateArrange(ref this._dock, value);
    }

    /// <summary>
    /// Gets or sets the background of the control.
    /// </summary>
    public IBackground? Background
    {
        get => this._background;
        set => this.SetProperty(ref this._background, value);
    }

    /// <summary>
    /// Gets or sets the width of the control. A value of <see cref="float.NaN"/> indicates that the width is determined by the layout process.
    /// </summary>
    public float Width
    {
        get => this._width;
        set => this.SetPropertyAndInvalidateMeasure(ref this._width, value);
    }

    /// <summary>
    /// Gets or sets the height of the control. A value of <see cref="float.NaN"/> indicates that the height is determined by the layout process.
    /// </summary>
    public float Height
    {
        get => this._height;
        set => this.SetPropertyAndInvalidateMeasure(ref this._height, value);
    }

    /// <summary>
    /// Gets or sets the distance between the left edge of the control and the left edge of its parent <see cref="Canvas"/>.
    /// </summary>
    public float Left
    {
        get => this._left;
        set => this.SetPropertyAndInvalidateMeasure(ref this._left, value);
    }

    /// <summary>
    /// Gets or sets the distance between the top edge of the control and the top edge of its parent <see cref="Canvas"/>.
    /// </summary>
    public float Top
    {
        get => this._top;
        set => this.SetPropertyAndInvalidateMeasure(ref this._top, value);
    }

    /// <summary>
    /// Gets or sets the distance between the right edge of the control and the right edge of its parent <see cref="Canvas"/>.
    /// </summary>
    public float Right
    {
        get => this._right;
        set => this.SetPropertyAndInvalidateMeasure(ref this._right, value);
    }

    /// <summary>
    /// Gets or sets the distance between the bottom edge of the control and the bottom edge of its parent <see cref="Canvas"/>.
    /// </summary>
    public float Bottom
    {
        get => this._bottom;
        set => this.SetPropertyAndInvalidateMeasure(ref this._bottom, value);
    }

    /// <summary>
    /// Gets or sets the tooltip that is displayed for this control.
    /// </summary>
    public Tooltip? Tooltip
    {
        get => this._tooltip;
        set => this.SetProperty(ref this._tooltip, value);
    }

    /// <summary>
    /// Gets or sets the padding applied to the hit test bounds of the control.
    /// This padding expands (positive values) or contracts (negative values) the area used for hit testing.
    /// </summary>
    public Thickness HitTestPadding { get; set; } = new Thickness(0);

    /// <summary>
    /// Initializes a new instance of the <see cref="Control"/> class.
    /// </summary>
    public Control() : base() { }

    /// <inheritdoc/>
    protected override Vector2 MeasureCore(Vector2 availableSize)
    {
        var margin = this.Margin.Total;
        availableSize -= margin;


        if (!float.IsNaN(this.Width)) availableSize.X = this.Width;
        if (!float.IsNaN(this.Height)) availableSize.Y = this.Height;

        Vector2 desiredSize = this.MeasureOverride(availableSize);

        if (!float.IsNaN(this.Width))
        {
            desiredSize.X = this.Width;
        }

        if (!float.IsNaN(this.Height))
        {
            desiredSize.Y = this.Height;
        }

        return desiredSize + margin;
    }

    /// <inheritdoc/>
    protected override Rect ArrangeCore(Rect finalRect)
    {
        finalRect = finalRect.Deflate(this.Margin);
        Vector2 availableSize = finalRect.Size;

        Vector2 desiredSize = this.DesiredSize - this.Margin.Total;

        switch (this.HorizontalAlignment)
        {
            case HorizontalAlignment.Center:
                finalRect.X += (availableSize.X - desiredSize.X) / 2;
                finalRect.Width = desiredSize.X;
                break;
            case HorizontalAlignment.Right:
                finalRect.X += availableSize.X - desiredSize.X;
                finalRect.Width = desiredSize.X;
                break;
            case HorizontalAlignment.Left:
                finalRect.Width = desiredSize.X;
                break;
            case HorizontalAlignment.Stretch:
                finalRect.Width = availableSize.X;
                break;
            default: throw new InvalidOperationException();
        }

        switch (this.VerticalAlignment)
        {
            case VerticalAlignment.Center:
                finalRect.Y += (availableSize.Y - desiredSize.Y) / 2;
                finalRect.Height = desiredSize.Y;
                break;
            case VerticalAlignment.Bottom:
                finalRect.Y += availableSize.Y - desiredSize.Y;
                finalRect.Height = desiredSize.Y;
                break;
            case VerticalAlignment.Top:
                finalRect.Height = desiredSize.Y;
                break;
            case VerticalAlignment.Stretch:
                finalRect.Height = availableSize.Y;
                break;
            default: throw new InvalidOperationException();
        }

        finalRect.Width = !float.IsNaN(this.Width) ? this.Width : MathF.Max(0, finalRect.Width);
        finalRect.Height = !float.IsNaN(this.Height) ? this.Height : MathF.Max(0, finalRect.Height);

        return this.ArrangeOverride(finalRect);
    }

    /// <inheritdoc/>
    protected override Rect GetBounds()
    {
        return new Rect(this.Position, this.ActualSize);
    }

    /// <summary>
    /// Gets the bounds used for hit testing.
    /// </summary>
    /// <returns>The bounds used for hit testing.</returns>
    protected virtual Rect GetHitTestBounds()
    {
        return this.GetBounds().Inflate(this.HitTestPadding);
    }

    /// <summary>
    /// Updates the control's state, called once per frame.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last frame, in seconds.</param>
    public virtual void Update(float deltaTime)
    {
        if (this.Stage == null) return;
        var input = this.Stage.Input;

        var mousePosition = this.Stage.WindowToViewport(input.CursorPosition);
        this.IsMouseOver = this.GetHitTestBounds().Contains(mousePosition);
    }

    /// <summary>
    /// Gets the parent of this control as a <see cref="Control"/>, or null if the parent is not a <see cref="Control"/> or if the control has no parent.
    /// </summary>
    protected Control? ControlParent => this.Parent as Control;

    /// <summary>
    /// Handles mouse button press events for this control.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    public virtual void HandleMousePressed(MouseButtonEventArgs e)
    {
        this.MouseDown?.Invoke(this, e);
        if (e.Handled) return;
        this.ControlParent?.HandleMousePressed(e);
    }

    /// <summary>
    /// Handles mouse button release events for this control.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    public virtual void HandleMouseReleased(MouseButtonEventArgs e)
    {
        this.MouseUp?.Invoke(this, e);
        if (e.Handled) return;
        this.ControlParent?.HandleMouseReleased(e);
    }

    /// <summary>
    /// Handles mouse wheel scroll events for this control.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    public virtual void HandleMouseWheel(MouseWheelEventArgs e)
    {
        this.MouseWheel?.Invoke(this, e);
        if (e.Handled) return;
        this.ControlParent?.HandleMouseWheel(e);
    }

    /// <summary>
    /// Called when the mouse enters the control's bounds.
    /// </summary>
    protected virtual void OnMouseEnter()
    {
        this.MouseEnter?.Invoke(this, EventArgs.Empty);

        if (this.Tooltip != null && this.Stage != null && this.Visibility == Visibility.Visible)
        {
            TooltipService.Instance.ShowTooltip(this);
        }
    }

    /// <summary>
    /// Called when the mouse leaves the control's bounds.
    /// </summary>
    protected virtual void OnMouseLeave()
    {
        this.MouseLeave?.Invoke(this, EventArgs.Empty);

        if (this.Tooltip != null)
        {
            TooltipService.Instance.HideTooltip(this);
        }
    }

    private bool _isMouseOver = false;

    /// <summary>
    /// Gets a value indicating whether the mouse pointer is currently over the control.
    /// </summary>
    public bool IsMouseOver
    {
        get => this._isMouseOver;
        private set
        {
            if (this._isMouseOver == value) return;

            this._isMouseOver = value;

            if (value)
            {
                this.OnMouseEnter();
            }
            else
            {
                this.OnMouseLeave();
            }
        }
    }

    /// <summary>
    /// Provides the core arrangement logic for derived classes. This method can be overridden
    /// to customize how the control positions and sizes its content within its allocated space.
    /// </summary>
    /// <param name="finalRect">
    /// The final area within the parent that this control should use to arrange itself and its children.
    /// </param>
    /// <returns>
    /// The actual size and position that the control occupies after arrangement.
    /// </returns>
    protected virtual Rect ArrangeOverride(Rect finalRect)
    {
        return finalRect;
    }

    /// <summary>
    /// Provides the core measuring logic for derived classes. This method can be overridden
    /// to customize how the control calculates its desired size based on the available space and its content.
    /// </summary>
    /// <param name="availableSize">
    /// The available size that this object can give to child objects. Infinity can be specified as a value to indicate that the object will size to whatever content is available.
    /// </param>
    /// <returns>
    /// The desired size of the control, including any padding or content size.
    /// </returns>
    protected virtual Vector2 MeasureOverride(Vector2 availableSize)
    {
        return availableSize;
    }

    /// <inheritdoc/>
    protected override void DrawCore(DrawingContext ctx)
    {
        this.Background?.DrawRectangle(ctx, this.Position, this.ActualSize);
    }

    /// <summary>
    /// Gets the children of this control that should be considered for hit testing.
    /// </summary>
    /// <summary>
    /// Gets a read-only list of child controls that should be considered when performing hit testing. Derived classes can override this property to specify which children participate in hit testing.
    /// </summary>
    protected virtual IReadOnlyList<Control> HitTestingChildren => [];


    /// <summary>
    /// Gets or sets a value indicating whether this control can be hit by input events.
    /// </summary>
    public bool IsHitTestVisible { get; set; } = true;

    /// <summary>
    /// Performs a hit test to find the control at the specified position.
    /// </summary>
    /// <param name="position">The position to test, in viewport space.</param>
    /// <returns>The topmost control at the specified position, or null if no control is found.</returns>
    public Control? HitTest(Vector2 position)
    {
        // if the element is not visible, we also remove all its children
        if (this.Visibility != Visibility.Visible) return null;
        if (!this.IsHitTestVisible) return null;

        bool hits = this.GetHitTestBounds().Contains(position);

        // Early exit if the position is outside the bounds of this control
        if (this.ClipToBounds && !hits) return null;

        // Recursively check child controls
        foreach (var child in this.HitTestingChildren)
        {
            var hitControl = child.HitTest(position);
            if (hitControl != null)
            {
                return hitControl;
            }
        }

        // If no child control was hit, return this control
        return hits && this.Background != null ? this : null;
    }
}
