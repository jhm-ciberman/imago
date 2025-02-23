using System;
using System.Numerics;
using LifeSim.Imago.Controls.Drawing;
using LifeSim.Imago.Input;
using LifeSim.Imago.Rendering.Sprites;
using LifeSim.Support.Numerics;
using Veldrid;

namespace LifeSim.Imago.Controls;

public class MouseButtonEventArgs : EventArgs
{
    public MouseButton Button { get; }
    public Vector2 Position { get; }

    public MouseButtonEventArgs(MouseButton button, Vector2 position)
    {
        this.Button = button;
        this.Position = position;
    }
}

public class MouseWheelEventArgs : EventArgs
{
    public float Delta { get; }
    public Vector2 Position { get; }

    public MouseWheelEventArgs(float delta, Vector2 position)
    {
        this.Delta = delta;
        this.Position = position;
    }
}

public class Control : Visual
{
    /// <summary>
    /// Occurs when the mouse enters the bounds of the control.
    /// </summary>
    public EventHandler? MouseEnter;

    /// <summary>
    /// Occurs when the mouse leaves the bounds of the control.
    /// </summary>
    public EventHandler? MouseLeave;

    /// <summary>
    /// Occurs when a mouse button is pressed down on the control.
    /// </summary>
    public EventHandler<MouseButtonEventArgs>? MouseDown;

    /// <summary>
    /// Occurs when a mouse button is released on the control.
    /// </summary>
    public EventHandler<MouseButtonEventArgs>? MouseUp;

    /// <summary>
    /// Occurs when the mouse wheel is scrolled while the mouse is over the control.
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

    /// <summary>
    /// Gets or sets the margin of the control.
    /// </summary>
    public Thickness Margin
    {
        get => this._margin;
        set => this.SetPropertyAndInvalidateMeasure(ref this._margin, value);
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of the control.
    /// </summary>
    public HorizontalAlignment HorizontalAlignment
    {
        get => this._horizontalAlignment;
        set => this.SetPropertyAndInvalidateMeasure(ref this._horizontalAlignment, value);
    }

    /// <summary>
    /// Gets or sets the vertical alignment of the control.
    /// </summary>
    public VerticalAlignment VerticalAlignment
    {
        get => this._verticalAlignment;
        set => this.SetPropertyAndInvalidateMeasure(ref this._verticalAlignment, value);
    }

    /// <summary>
    /// Gets or sets the dock position of the control.
    /// </summary>
    public Dock Dock
    {
        get => this._dock;
        set => this.SetPropertyAndInvalidateArrange(ref this._dock, value);
    }

    /// <summary>
    /// Gets or sets the background brush of the control.
    /// </summary>
    public IBackground? Background
    {
        get => this._background;
        set => this.SetProperty(ref this._background, value);
    }

    /// <summary>
    /// Gets or sets the width of the control. A value of float.NaN indicates that the width should be calculated automatically using the control's content.
    /// </summary>
    public float Width
    {
        get => this._width;
        set => this.SetPropertyAndInvalidateMeasure(ref this._width, value);
    }

    /// <summary>
    /// Gets or sets the height of the control. A value of float.NaN indicates that the height should be calculated automatically using the control's content.
    /// </summary>
    public float Height
    {
        get => this._height;
        set => this.SetPropertyAndInvalidateMeasure(ref this._height, value);
    }

    // left, top, right, bottom

    /// <summary>
    /// Gets or sets the left position of the control. This is only relevant when positioning the control inside a <see cref="Canvas"/>.
    /// </summary>
    public float Left
    {
        get => this._left;
        set => this.SetPropertyAndInvalidateMeasure(ref this._left, value);
    }

    /// <summary>
    /// Gets or sets the top position of the control. This is only relevant when positioning the control inside a <see cref="Canvas"/>.
    /// </summary>
    public float Top
    {
        get => this._top;
        set => this.SetPropertyAndInvalidateMeasure(ref this._top, value);
    }

    /// <summary>
    /// Gets or sets the right position of the control. This is only relevant when positioning the control inside a <see cref="Canvas"/>.
    /// </summary>
    public float Right
    {
        get => this._right;
        set => this.SetPropertyAndInvalidateMeasure(ref this._right, value);
    }

    /// <summary>
    /// Gets or sets the bottom position of the control. This is only relevant when positioning the control inside a <see cref="Canvas"/>.
    /// </summary>
    public float Bottom
    {
        get => this._bottom;
        set => this.SetPropertyAndInvalidateMeasure(ref this._bottom, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Control"/> class.
    /// </summary>
    public Control() : base() { }

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
        };

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
        };

        finalRect.Width = !float.IsNaN(this.Width) ? this.Width : MathF.Max(0, finalRect.Width);
        finalRect.Height = !float.IsNaN(this.Height) ? this.Height : MathF.Max(0, finalRect.Height);

        return this.ArrangeOverride(finalRect);
    }

    protected override Rect GetBounds()
    {
        return new Rect(this.Position, this.ActualSize);
    }

    /// <summary>
    /// Updates the control. This method is called by the <see cref="Visual.Stage"/> of the control in each frame.
    /// </summary>
    public virtual void Update(float deltaTime)
    {
        if (this.Stage == null) return;
        var input = this.Stage.Input;

        var bounds = this.GetBounds();
        var mousePosition = this.Stage.WindowToViewport(input.MousePosition);
        this.IsMouseOver = bounds.Contains(mousePosition);

        if (this.IsMouseOver)
        {
            this.CheckMouseButtonEvents(input, MouseButton.Left, mousePosition);
            this.CheckMouseButtonEvents(input, MouseButton.Right, mousePosition);
            this.CheckMouseButtonEvents(input, MouseButton.Middle, mousePosition);

            float delta = input.MouseWheelDelta;

            if (delta != 0)
            {
                this.OnMouseWheel(delta, mousePosition);
            }
        }
    }

    private void CheckMouseButtonEvents(InputManager input, MouseButton button, Vector2 mousePosition)
    {
        if (input.GetMouseButtonDown(button))
        {
            this.OnMouseDown(button, mousePosition);
        }
        else if (input.GetMouseButtonUp(button))
        {
            this.OnMouseUp(button, mousePosition);
        }
    }

    protected virtual void OnMouseDown(MouseButton button, Vector2 mousePosition)
    {
        this.MouseDown?.Invoke(this, new MouseButtonEventArgs(button, mousePosition));
    }

    protected virtual void OnMouseUp(MouseButton button, Vector2 mousePosition)
    {
        this.MouseUp?.Invoke(this, new MouseButtonEventArgs(button, mousePosition));
    }

    protected virtual void OnMouseWheel(float delta, Vector2 mousePosition)
    {
        this.MouseWheel?.Invoke(this, new MouseWheelEventArgs(delta, mousePosition));
    }

    protected virtual void OnMouseEnter()
    {
        this.MouseEnter?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnMouseLeave()
    {
        this.MouseLeave?.Invoke(this, EventArgs.Empty);
    }

    private bool _isMouseOver = false;

    /// <summary>
    /// Gets a value indicating whether the mouse is currently over the control.
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

    protected virtual Rect ArrangeOverride(Rect finalRect)
    {
        return finalRect;
    }

    protected virtual Vector2 MeasureOverride(Vector2 availableSize)
    {
        return availableSize;
    }

    protected override void DrawCore(DrawingContext ctx)
    {
        this.Background?.DrawRectangle(ctx, this.Position, this.ActualSize);
    }
}
