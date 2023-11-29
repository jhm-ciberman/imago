using System.Numerics;
using LifeSim.Imago.Controls.Drawing;
using LifeSim.Imago.Graphics.Rendering;
using LifeSim.Imago.Input;
using LifeSim.Support.Drawing;
using LifeSim.Support.Numerics;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Defines the scroll direction.
/// </summary>
public enum ScrollDirection
{
    None = 0,
    Horizontal = 1,
    Vertical = 2,
}

public class ScrollViewer : ContentControl
{
    private ScrollDirection _scrollDirection = ScrollDirection.Vertical;

    /// <summary>
    /// Gets or sets the direction of the scroll.
    /// </summary>
    public ScrollDirection ScrollDirection
    {
        get => this._scrollDirection;
        set => this.SetPropertyAndInvalidateMeasure(ref this._scrollDirection, value);
    }

    private Vector2 _scrollOffset;

    /// <summary>
    /// Gets or sets the scroll viewer's scroll offset.
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
    /// Gets or sets the aceleration in pixels/s^2 that the scroll viewer will use to scroll when the user scrolls with the mouse wheel one unit.
    /// </summary>
    public float MouseWheelAcceleration { get; set; } = 150f;

    /// <summary>
    /// Gets or sets the scroll friction. This is how much the content speed will be reduced when the user stops scrolling in each frame.
    /// </summary>
    public float ScrollFriction { get; set; } = 0.85f;

    /// <summary>
    /// Gets or sets the scroll viewer's current scroll speed.
    /// </summary>
    public Vector2 ScrollSpeed { get; set; } = Vector2.Zero;

    /// <summary>
    /// Gets or sets the scroll percentage from 0 to 1 in the horizontal axis.
    /// </summary>
    public float ScrollPercentageX
    {
        get => this.ScrollOffset.X / this.ScrollableSize.X;
        set => this.ScrollOffset = new Vector2(value * this.ScrollableSize.X, this.ScrollOffset.Y);
    }

    /// <summary>
    /// Gets or sets the scroll percentage from 0 to 1 in the vertical axis.
    /// </summary>
    public float ScrollPercentageY
    {
        get => this.ScrollOffset.Y / this.ScrollableSize.Y;
        set => this.ScrollOffset = new Vector2(this.ScrollOffset.X, value * this.ScrollableSize.Y);
    }

    private Control? _scrollBarThumb = null;

    /// <summary>
    /// Gets or sets the scroll viewer's scroll bar thumb element.
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

    public ScrollViewer() : base()
    {
        this.ClipToBounds = true;
    }

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

    protected override Rect ArrangeOverride(Rect finalRect)
    {
        if (this.Content != null)
        {
            Rect rect = new Rect(finalRect.Position - this._scrollOffset, Vector2.Min(this.Content.DesiredSize, finalRect.Size));
            this.Content.Arrange(rect);
            this.ActualSize = finalRect.Size; // I set up the ActualSize here so it can be used by "OnScrollChanged"
            this.ScrollBarThumb.Arrange(finalRect);
            this.OnScrollChanged();
        }

        return finalRect;
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        base.DrawCore(spriteBatcher);

        var thumb = this.ScrollBarThumb;
        if (thumb.Visibility == Visibility.Visible)
        {
            thumb.Draw(spriteBatcher);
        }
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        float wheelDelta = -InputManager.Current.MouseWheelDelta;

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

    public override void OnAddedToStage(GuiLayer stage)
    {
        base.OnAddedToStage(stage);
        this.ScrollBarThumb.OnAddedToStage(stage);
    }

    public override void OnRemovedFromStage(GuiLayer stage)
    {
        base.OnRemovedFromStage(stage);
        this.ScrollBarThumb.OnRemovedFromStage(stage);
    }

    /// <summary>
    /// Gets the scrollable size of the scroll viewer. This is the size of the scroll viewer minus the size of the scroll viewer's content.
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
