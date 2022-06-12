using System;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Controls;

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
    /// <summary>
    /// Gets or sets the direction of the scroll.
    /// </summary>
    public ScrollDirection ScrollDirection { get; set; } = ScrollDirection.Vertical;

    private Vector2 _scrollOffset;

    /// <summary>
    /// Gets or sets the scroll viewer's scroll offset.
    /// </summary>
    public Vector2 ScrollOffset
    {
        get => this._scrollOffset;
        set
        {
            if (this._scrollOffset != value)
            {
                this._scrollOffset = value;
                if (this.Content != null)
                {
                    this.Content.Transform = Matrix3x2.CreateTranslation(-this._scrollOffset.X, -this._scrollOffset.Y);
                }
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
            Background = new SolidColorBrush(Color.Green),
        };
        return this._scrollBarThumb;
    }

    public ScrollViewer() : base()
    {
        this.ClipToBounds = true;
    }

    protected override Vector2 MeasureCore(Vector2 availableSize)
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

        return base.MeasureCore(availableSize);
    }

    protected override Rect ArrangeCore(Rect finalRect)
    {
        if (this.Content != null)
        {
            Rect rect = new Rect(finalRect.Position, Vector2.Min(this.Content.DesiredSize, finalRect.Size));
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

    private void OnScrollChanged()
    {
        var thumb = this.ScrollBarThumb;
        Vector2 scrollableSize = this.ScrollableSize;
        float scrollableSizeInAxis = this.ScrollDirection == ScrollDirection.Horizontal ? scrollableSize.X : scrollableSize.Y;
        if (this.Content != null && scrollableSizeInAxis > 0 && this.ScrollDirection != ScrollDirection.None)
        {
            this.ScrollOffset = Vector2.Min(Vector2.Max(Vector2.Zero, this.ScrollOffset), scrollableSize);
            thumb.Visibility = Visibility.Visible;

            // Modify scroll bar thumb position (thumb.Transform matrix)
            Vector2 thumbSize = thumb.ActualSize + thumb.Margin.Total;

            float x, y;
            if (this.ScrollDirection == ScrollDirection.Vertical)
            {
                x = this.ActualSize.X - thumbSize.X;
                y = this.ScrollPercentageY * (this.ActualSize.Y - thumbSize.Y);
            }
            else
            {
                x = this.ScrollPercentageX * (this.ActualSize.X - thumbSize.X);
                y = this.ActualSize.Y - thumbSize.Y;
            }
            thumb.Transform = Matrix3x2.CreateTranslation(x, y);
        }
        else
        {
            this.ScrollOffset = Vector2.Zero;
            thumb.Visibility = Visibility.Collapsed;
        }
    }
}