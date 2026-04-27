using System.Collections.Generic;
using System.Numerics;
using Imago.Rendering.Sprites;
using Imago.Support.Numerics;

namespace Imago.Controls;

/// <summary>
/// Represents a control that can contain a single child control.
/// </summary>
[ContentProperty(nameof(Content))]
public class ContentControl : Control
{
    private Thickness _padding = new Thickness(0);

    /// <summary>
    /// Gets or sets the padding of the content control, which is the space between the control's border and its content.
    /// </summary>
    public Thickness Padding
    {
        get => this._padding;
        set => this.SetPropertyAndInvalidateMeasure(ref this._padding, value);
    }

    private Control? _content;
    private readonly Control[] _hitTestChildren = [null!];

    /// <summary>
    /// Gets or sets the single child control contained within this <see cref="ContentControl"/>.
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
                this._hitTestChildren[0] = value!;

                if (this._content != null)
                {
                    this.AddVisualChild(this._content);
                }

                this.InvalidateMeasure();
            }
        }
    }

    private object? _contentSource;

    /// <summary>
    /// Gets or sets the data item rendered by this <see cref="ContentControl"/> through <see cref="ContentTemplate"/>.
    /// </summary>
    /// <remarks>
    /// When this property changes, the previous <see cref="Content"/> is disposed and a new control is built
    /// by invoking <see cref="ContentTemplate"/> on the new value. Setting <see langword="null"/> clears the content.
    /// If <see cref="ContentTemplate"/> is also <see langword="null"/>, no control is built.
    /// </remarks>
    public object? ContentSource
    {
        get => this._contentSource;
        set
        {
            if (this._contentSource == value) return;
            this._contentSource = value;
            this.RebuildContentFromSource();
        }
    }

    private IDataTemplate? _contentTemplate;

    /// <summary>
    /// Gets or sets the data template used to build a control from <see cref="ContentSource"/>.
    /// </summary>
    /// <remarks>
    /// Assigning a new template rebuilds the current content from <see cref="ContentSource"/>.
    /// Use <see cref="DataTemplates"/> to dispatch to different controls based on the source's runtime type.
    /// </remarks>
    public IDataTemplate? ContentTemplate
    {
        get => this._contentTemplate;
        set
        {
            if (this._contentTemplate == value) return;
            this._contentTemplate = value;
            this.RebuildContentFromSource();
        }
    }

    private void RebuildContentFromSource()
    {
        var oldContent = this._content;

        Control? newContent = null;
        if (this._contentSource != null && this._contentTemplate != null)
        {
            newContent = this._contentTemplate.CreateItem(this._contentSource);
        }

        this.Content = newContent;

        if (oldContent != null && oldContent != newContent)
        {
            oldContent.Dispose();
        }
    }

    /// <inheritdoc/>
    protected override IReadOnlyList<Control> HitTestingChildren
    {
        get => this.Content != null ? this._hitTestChildren : [];
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override Rect ArrangeOverride(Rect finalRect)
    {
        if (this.Content != null)
        {
            Rect contentRect = finalRect.Deflate(this.Padding);
            this.Content.Arrange(contentRect);
        }

        return finalRect;
    }

    /// <inheritdoc/>
    protected override void DrawCore(DrawingContext ctx)
    {
        base.DrawCore(ctx);

        this.Content?.Draw(ctx);
    }

    /// <inheritdoc/>
    public override void Update(float deltaTime)
    {
        this.Content?.Update(deltaTime);

        base.Update(deltaTime);
    }
}
