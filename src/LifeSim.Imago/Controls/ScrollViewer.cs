using System.Numerics;
using LifeSim.Imago.Controls.Drawing;
using LifeSim.Imago.Rendering.Sprites;
using LifeSim.Support.Drawing;
using LifeSim.Support.Numerics;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Defines the scroll direction.
/// </summary>
public enum ScrollDirection
{
    /// <summary>
    /// No scrolling is enabled.
    /// </summary>
    None = 0,

    /// <summary>
    /// Horizontal scrolling is enabled.
    /// </summary>
    Horizontal = 1,

    /// <summary>
    /// Vertical scrolling is enabled.
    /// </summary>
    Vertical = 2,
}

/// <summary>
/// Represents a scrollable content control that enables users to pan and scroll content that extends beyond the visible area.
/// </summary>
public class ScrollViewer : ContentControl
{
    private ScrollDirection _scrollDirection = ScrollDirection.Vertical;

    /// <summary>
    /// Gets or sets the direction in which the content can be scrolled.
    /// </summary>
    public ScrollDirection ScrollDirection
    {
        get => this._scrollDirection;
        set => this.SetPropertyAndInvalidateMeasure(ref this._scrollDirection, value);
    }

    private Vector2 _scrollOffset;

    /// <summary>
    /// Gets or sets the current scroll offset of the content within the viewer.
    /// </summary>
    public Vector2 ScrollOffset
    {
        get => this._scrollOffset;
        set
        {
            if (this.Content == null)
            {
                value = Vector2.Zero;
            }

            if (this._scrollOffset != value)
            {
                this._scrollOffset = value;
                this.InvalidateArrange();
            }
        }
    }

    /// <summary>
    /// Gets or sets the acceleration in pixels/s^2 that the scroll viewer will use to scroll when the user scrolls with the mouse wheel one unit.
    /// </summary>
    public float MouseWheelAcceleration { get; set; } = 150f;

    /// <summary>
    /// Gets or sets the friction applied to scrolling speed. A value closer to 0 means more friction (faster deceleration).
    /// </summary>
    public float ScrollFriction { get; set; } = 0.85f;

    /// <summary>
    /// Gets or sets the current scrolling speed of the content.
    /// </summary>
    public Vector2 ScrollSpeed { get; set; } = Vector2.Zero;

    /// <summary>
    /// Gets or sets the horizontal scroll percentage (0.0 to 1.0).
    /// </summary>
    public float ScrollPercentageX
    {
        get => this.ScrollOffset.X / this.ScrollableSize.X;
        set => this.ScrollOffset = new Vector2(value * this.ScrollableSize.X, this.ScrollOffset.Y);
    }

    /// <summary>
    /// Gets or sets the vertical scroll percentage (0.0 to 1.0).
    /// </summary>
    public float ScrollPercentageY
    {
        get => this.ScrollOffset.Y / this.ScrollableSize.Y;
        set => this.ScrollOffset = new Vector2(this.ScrollOffset.X, value * this.ScrollableSize.Y);
    }

    private Control? _scrollBarThumb = null;

    /// <summary>
    /// Gets or sets the control used as the scroll bar thumb. If null, a default thumb is created.
    /// </summary>
    public Control ScrollBarThumb
    {
        get => this._scrollBarThumb ?? this.CreateDefaultScrollBarThumb();
        set => this._scrollBarThumb = value;
    }

    private Control CreateDefaultScrollBarThumb()
    {
        this._scrollBarThumb = new Control
        {
            Width = 5,
            Height = 5,
            Margin = new Thickness(5),
            Background = new ColorBackground(Color.Green),
        };
        return this._scrollBarThumb;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollViewer"/> class.
    /// </summary>
    public ScrollViewer() : base()
    {
        this.ClipToBounds = true;
        this.HorizontalAlignment = HorizontalAlignment.Stretch;
        this.VerticalAlignment = VerticalAlignment.Stretch;
    }

    /// <inheritdoc/>
    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        if (this.ScrollDirection == ScrollDirection.Horizontal)
        {
            availableSize.X = float.PositiveInfinity;
        }
        else if (this.ScrollDirection == ScrollDirection.Vertical)
        {
            availableSize.Y = float.PositiveInfinity;
        }

        this.ScrollBarThumb.Measure(availableSize);

        return base.MeasureOverride(availableSize);
    }

    /// <inheritdoc/>
    protected override Rect ArrangeOverride(Rect finalRect)
    {
        if (this.Content != null)
        {
            Rect rect = new Rect(finalRect.Position - this._scrollOffset, this.Content.DesiredSize);
            this.Content.Arrange(rect);
            this.ActualSize = finalRect.Size; // I set up the ActualSize here so it can be used by "OnScrollChanged"
            this.ScrollBarThumb.Arrange(finalRect);
            this.OnScrollChanged();
        }

        return finalRect;
    }

    /// <inheritdoc/>
    protected override void DrawCore(DrawingContext ctx)
    {
        base.DrawCore(ctx);

        var thumb = this.ScrollBarThumb;
        if (thumb.Visibility == Visibility.Visible)
        {
            thumb.Draw(ctx);
        }
    }

    /// <inheritdoc/>
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        float wheelDelta = -this.Layer?.Input.MouseScrollDelta ?? 0;

        if (wheelDelta != 0)
        {
            // Vertical has priority over horizontal
            if (this.ScrollDirection == ScrollDirection.Vertical)
            {
                this.ScrollSpeed += new Vector2(0, wheelDelta) * this.MouseWheelAcceleration;
            }
            else
            {
                this.ScrollSpeed += new Vector2(wheelDelta, 0) * this.MouseWheelAcceleration;
            }
        }

        if (this.ScrollSpeed != Vector2.Zero)
        {
            this.ScrollSpeed *= this.ScrollFriction;
            this.ScrollOffset += this.ScrollSpeed * deltaTime;

            this.OnScrollChanged();
        }
    }

    /// <inheritdoc/>
    public override void OnAddedToLayer(GuiLayer layer)
    {
        base.OnAddedToLayer(layer);
        this.ScrollBarThumb.OnAddedToLayer(layer);
    }

    /// <inheritdoc/>
    public override void OnRemovedFromLayer(GuiLayer layer)
    {
        base.OnRemovedFromLayer(layer);
        this.ScrollBarThumb.OnRemovedFromLayer(layer);
    }

    /// <summary>
    /// Gets the total size of the content that is currently outside the visible area of the scroll viewer.
    /// </summary>
    public Vector2 ScrollableSize
    {
        get
        {
            if (this.Content == null) return Vector2.Zero;

            Vector2 contentSize = this.Content.ActualSize + this.Content.Margin.Total;
            return Vector2.Max(Vector2.Zero, contentSize - this.ActualSize);
        }
    }

    //private Vector2 _thumbPosition;

    private void OnScrollChanged()
    {
        if (this.Content == null)
        {
            this.ScrollOffset = Vector2.Zero;
            this.ScrollBarThumb.Visibility = Visibility.Collapsed;
            return;
        }

        if (this.ScrollDirection == ScrollDirection.None)
        {
            this.ScrollBarThumb.Visibility = Visibility.Collapsed;
            return;
        }

        var thumb = this.ScrollBarThumb;
        Vector2 scrollableSize = this.ScrollableSize;
        float scrollableLength = this.ScrollDirection == ScrollDirection.Horizontal ? scrollableSize.X : scrollableSize.Y;
        if (scrollableLength > 0 && this.ScrollDirection != ScrollDirection.None)
        {
            this.ScrollOffset = Vector2.Min(Vector2.Max(Vector2.Zero, this.ScrollOffset), scrollableSize);
            thumb.Visibility = Visibility.Visible;

            // Modify scroll bar thumb position (thumb.Transform matrix)
            Vector2 thumbSize = thumb.ActualSize + thumb.Margin.Total;

            Vector2 pos;
            if (this.ScrollDirection == ScrollDirection.Vertical)
            {
                pos.X = this.ActualSize.X - thumbSize.X;
                pos.Y = this.ScrollPercentageY * (this.ActualSize.Y - thumbSize.Y);
            }
            else
            {
                pos.X = this.ScrollPercentageX * (this.ActualSize.X - thumbSize.X);
                pos.Y = this.ActualSize.Y - thumbSize.Y;
            }
            //this._thumbPosition = pos;
        }
        else
        {
            //this.ScrollOffset = Vector2.Zero;
            thumb.Visibility = Visibility.Collapsed;
        }
    }
}
