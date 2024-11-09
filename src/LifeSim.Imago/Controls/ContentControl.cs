using System.Numerics;
using LifeSim.Imago.Rendering.Sprites;
using LifeSim.Support.Numerics;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Represents a control that can contain one other control.
/// </summary>
public class ContentControl : Control
{
    private Thickness _padding = new Thickness(0);

    /// <summary>
    /// Gets or sets the padding of the content control.
    /// </summary>
    public Thickness Padding
    {
        get => this._padding;
        set => this.SetPropertyAndInvalidateMeasure(ref this._padding, value);
    }

    private Control? _content;

    /// <summary>
    /// Gets or sets the content of the control.
    /// </summary>
    public Control? Content
    {
        get => this._content;
        set
        {
            if (this._content != value)
            {
                var oldContent = this._content;
                if (oldContent != null)
                {
                    this.RemoveVisualChild(oldContent);
                }

                this._content = value;

                if (this._content != null)
                {
                    this.AddVisualChild(this._content);
                }

                this.InvalidateMeasure();
            }
        }
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        if (this.Content != null)
        {
            availableSize -= this.Padding.Total;
            this.Content.Measure(availableSize);
            return this.Content.DesiredSize + this.Padding.Total;
        }
        else
        {
            return Vector2.Zero;
        }
    }

    protected override Rect ArrangeOverride(Rect finalRect)
    {
        if (this.Content != null)
        {
            Rect contentRect = finalRect.Deflate(this.Padding);
            this.Content.Arrange(contentRect);
        }

        return finalRect;
    }

    protected override void DrawCore(DrawingContext ctx)
    {
        base.DrawCore(ctx);

        this.Content?.Draw(ctx);
    }

    public override void Update(float deltaTime)
    {
        this.Content?.Update(deltaTime);

        base.Update(deltaTime);
    }
}
