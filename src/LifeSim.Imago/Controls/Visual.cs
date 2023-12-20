using System;
using System.Collections.Generic;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using LifeSim.Imago.Graphics.Rendering;
using LifeSim.Support.Numerics;

namespace LifeSim.Imago.Controls;

public abstract class Visual : ObservableObject, IDisposable
{
    private string _name = string.Empty;
    private Visibility _visibility = Visibility.Visible;
    private GuiLayer? _stage;
    private float _opacity = 1f;
    private readonly List<Visual> _visualChildren = new List<Visual>();
    private bool _clipToBounds = false;
    private IStyle? _style;

    /// <summary>
    /// Gets or sets the name of the element.
    /// </summary>
    public string Name
    {
        get => this._name;
        set => this.SetProperty(ref this._name, value);
    }


    /// <summary>
    /// Gets or sets the visibility of the control.
    /// </summary>
    public Visibility Visibility
    {
        get => this._visibility;
        set => this.SetPropertyAndInvalidateMeasure(ref this._visibility, value);
    }

    /// <summary>
    /// Gets the stage the control is added to, or null if the control is not added yet to a stage.
    /// </summary>

    public GuiLayer? Stage
    {
        get => this._stage;
        private set => this.SetProperty(ref this._stage, value);
    }


    /// <summary>
    /// Gets or sets the opacity of the control.
    /// </summary>
    public float Opacity
    {
        get => this._opacity;
        set => this.SetProperty(ref this._opacity, value);
    }

    /// <summary>
    /// Gets an enumerable collection of the control's visual children.
    /// </summary>
    public virtual IReadOnlyList<Visual> VisualChildren => this._visualChildren;

    /// <summary>
    /// Gets or sets whether the content is clipped to the control's bounds.
    /// </summary>
    public bool ClipToBounds
    {
        get => this._clipToBounds;
        set => this.SetProperty(ref this._clipToBounds, value);
    }

    /// <summary>
    /// Gets or sets the style of the control.
    /// </summary>
    public IStyle? Style
    {
        get => this._style;
        set
        {
            if (this._style != value)
            {
                this._style = value;
                this._style?.Apply(this);
                this.OnPropertyChanged(nameof(this.Style));
            }
        }
    }

    /// <summary>
    /// Gets whether the measure pass is valid.
    /// </summary>
    public bool IsMeasureValid { get; private set; } = false;

    /// <summary>
    /// Gets whether the arrange pass is valid.
    /// </summary>
    public bool IsArrangeValid { get; private set; } = false;

    /// <summary>
    /// Gets the parent of the control or null if the control has no parent.
    /// </summary>
    public Visual? Parent { get; internal set; }


    /// <summary>
    /// Gets the position of the control.
    /// </summary>
    public Vector2 Position { get; protected set; } = Vector2.Zero;

    /// <summary>
    /// Gets the actual size of the control. That is, the real size the control takes up.
    /// </summary>
    public Vector2 ActualSize { get; protected set; } = Vector2.Zero;

    /// <summary>
    /// Gets the desired size of the control. That is the size that the control has requested to take up after the measure pass.
    /// </summary>
    public Vector2 DesiredSize { get; private set; } = Vector2.Zero;

    /// <summary>
    /// Initializes a new instance of the <see cref="Visual"/> class.
    /// </summary>
    public Visual()
    {
        //
    }

    /// <summary>
    /// Draws the control.
    /// </summary>
    /// <param name="spriteBatcher">The sprite batch to use for drawing.</param>
    public void Draw(SpriteBatcher spriteBatcher)
    {
        if (this.Visibility != Visibility.Visible || this.Opacity <= 0f)
        {
            return;
        }

        if (this.Opacity < 1f)
        {
            spriteBatcher.PushOpacity(this.Opacity);
        }

        if (this.ClipToBounds)
        {
            spriteBatcher.PushScissorRectangle(this.GetBounds() * this.Stage!.Zoom);
            this.DrawCore(spriteBatcher);
            spriteBatcher.PopScissorRectangle();
        }
        else
        {
            this.DrawCore(spriteBatcher);
        }

        if (this.Opacity < 1f)
        {
            spriteBatcher.PopOpacity();
        }
    }

    protected abstract Rect GetBounds();

    protected abstract void DrawCore(SpriteBatcher spriteBatcher);

    /// <summary>
    /// Performs the measure pass of the layout process. In the measure pass, the control computes the desired size of the control
    /// and updates the <see cref="DesiredSize"/> property.
    /// </summary>
    /// <param name="availableSize">The available size that this object can give to child objects. Infinity can be specified as a value to indicate that the object will size to whatever content is available.</param>
    public void Measure(Vector2 availableSize)
    {
        if (this.IsMeasureValid) return;
        this.IsMeasureValid = true;

        if (this.Visibility == Visibility.Collapsed)
        {
            this.DesiredSize = Vector2.Zero;
            return;
        }

        this.DesiredSize = this.MeasureCore(availableSize);
    }

    protected abstract Vector2 MeasureCore(Vector2 availableSize);

    /// <summary>
    /// Performs the arrange pass of the layout process. In the arrange pass, the control positions its children and computes the actual size of the control.
    /// </summary>
    /// <param name="finalSize">The final size that this object should use to arrange itself and its children.</param>
    public void Arrange(Rect finalRect)
    {
        if (this.IsArrangeValid) return;
        this.IsArrangeValid = true;

        if (this.Visibility == Visibility.Collapsed)
        {
            this.ActualSize = Vector2.Zero;
            return;
        }

        finalRect = this.ArrangeCore(finalRect);

        // snap to pixels
        var position = finalRect.Position;
        var size = finalRect.Size;

        if (this.Stage!.SnapToPixels)
        {
            position.X = (float)Math.Round(position.X);
            position.Y = (float)Math.Round(position.Y);
            size.X = (float)Math.Round(size.X);
            size.Y = (float)Math.Round(size.Y);
        }

        this.Position = position;
        this.ActualSize = size;
    }

    protected abstract Rect ArrangeCore(Rect finalRect);

    private IDisposable? _bindings = null;

    public virtual void OnAddedToStage(GuiLayer stage)
    {
        if (this.Stage != null)
        {
            throw new InvalidOperationException("The control is already added to a page.");
        }

        this.IsArrangeValid = false;
        this.IsMeasureValid = false;
        this.Stage = stage;
        foreach (var child in this.VisualChildren)
        {
            child.OnAddedToStage(stage);
        }

        this._bindings = this.CreateBindings();
    }

    /// <summary>
    /// This method can be overridden to create bindings when the control is added to a stage.
    /// </summary>
    /// <returns>The created bindings.</returns>
    protected virtual IDisposable? CreateBindings()
    {
        return null;
    }

    public virtual void OnRemovedFromStage(GuiLayer stage)
    {
        if (this.Stage != stage)
        {
            throw new InvalidOperationException("The control is not added to the specified page.");
        }

        this.Stage = null;
        foreach (var child in this.VisualChildren)
        {
            child.OnRemovedFromStage(stage);
        }

        this._bindings?.Dispose();
    }

    /// <summary>
    /// Finds a child control of the specified type by its name recursively.
    /// </summary>
    /// <typeparam name="T">The type of the control to find.</typeparam>
    /// <param name="name">The name of the control to find.</param>
    /// <returns>The control if found, otherwise null.</returns>
    public T? Find<T>(string name) where T : Visual
    {
        if (this.Name == name)
        {
            return (T)this;
        }

        foreach (var child in this.VisualChildren)
        {
            var result = child.Find<T>(name);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds a child control of the specified type by its name recursively. Throws an exception if the control could not be found.
    /// </summary>
    /// <typeparam name="T">The type of the control to find.</typeparam>
    /// <param name="name">The name of the control to find.</param>
    /// <returns>The found control.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the control could not be found.</exception>
    public T FindOrFail<T>(string name) where T : Visual
    {
        return this.Find<T>(name) ?? throw new InvalidOperationException($"Could not find control with name '{name}'.");
    }

    protected virtual void AddVisualChild(Visual child)
    {
        this._visualChildren.Add(child);
        child.Parent = this;
        if (this.Stage != null)
        {
            child.OnAddedToStage(this.Stage);
            this.InvalidateMeasure();
        }
    }

    protected virtual void RemoveVisualChild(Visual child)
    {
        this._visualChildren.Remove(child);
        if (this.Stage != null)
        {
            child.OnRemovedFromStage(this.Stage);
            this.InvalidateMeasure();
        }
    }

    /// <summary>
    /// Sets a property and invalidates the measure of the control.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">The field to set.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>True if the value changed, false otherwise.</returns>
    protected bool SetPropertyAndInvalidateMeasure<T>(ref T field, T value)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            this.InvalidateMeasure();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Sets a property and invalidates the arrange of the control.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">The field to set.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>True if the value changed, false otherwise.</returns>
    protected bool SetPropertyAndInvalidateArrange<T>(ref T field, T value)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            this.InvalidateArrange();
            return true;
        }

        return false;
    }

    protected internal void InvalidateMeasure()
    {
        this.ForceMeasure();
        this.PropagateInvalidMeasureToChildren();
    }

    private void PropagateInvalidMeasureToChildren()
    {
        foreach (var child in this.VisualChildren)
        {
            if (!child.IsMeasureValid) continue;

            child.IsMeasureValid = false;
            child.IsArrangeValid = false;
            child.PropagateInvalidMeasureToChildren();
        }
    }

    private void ForceMeasure()
    {
        if (!this.IsMeasureValid) return;

        this.IsMeasureValid = false;
        this.IsArrangeValid = false;

        this.Parent?.InvalidateMeasure();
    }

    protected internal void InvalidateArrange()
    {
        this.ForceArrange();
        this.PropagateInvalidArrangeToChildren();
    }

    private void PropagateInvalidArrangeToChildren()
    {
        foreach (var child in this.VisualChildren)
        {
            if (!child.IsArrangeValid) continue;

            child.IsArrangeValid = false;
            child.PropagateInvalidArrangeToChildren();
        }
    }

    private void ForceArrange()
    {
        if (!this.IsArrangeValid) return;

        this.IsArrangeValid = false;

        this.Parent?.InvalidateArrange();
    }

    public bool IsDisposed { get; private set; } = false;

    /// <summary>
    /// Disposes the control.
    /// </summary>
    public void Dispose()
    {
        if (this.IsDisposed) return;
        this.IsDisposed = true;

        foreach (var child in this.VisualChildren)
        {
            child.Dispose();
        }

        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Visual() => this.Dispose(false);

    /// <summary>
    /// Disposes the control.
    /// </summary>
    /// <param name="disposing">Whether the control is disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        //
    }
}
