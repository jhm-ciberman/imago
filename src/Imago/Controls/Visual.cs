using System;
using System.Collections.Generic;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using FontStashSharp;
using Imago.Rendering.Sprites;
using Imago.SceneGraph;
using Imago.Support.Numerics;

namespace Imago.Controls;

/// <summary>
/// Represents the base class for all visual elements in the user interface.
/// </summary>
public abstract class Visual : ObservableObject, IDisposable, IMountable
{
    /// <summary>
    /// Gets or sets the default font system used by all controls.
    /// If not set, controls must manually define a font system to use for text rendering.
    /// </summary>
    public static FontSystem? DefaultFontSystem { get; set; } = null!;

    private string _name = string.Empty;
    private Visibility _visibility = Visibility.Visible;
    private GuiLayer? _layer;
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
    /// Gets the layer the control belongs to, or <see langword="null"/> if the control is not mounted.
    /// </summary>
    public GuiLayer? Layer
    {
        get => this._layer;
        private set => this.SetProperty(ref this._layer, value);
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
    /// <param name="ctx">The drawing context to use for drawing.</param>
    public void Draw(DrawingContext ctx)
    {
        if (this.Visibility != Visibility.Visible || this.Opacity <= 0f)
        {
            return;
        }

        if (this.Opacity < 1f)
        {
            ctx.PushOpacity(this.Opacity);
        }

        if (this.ClipToBounds)
        {
            ctx.PushScissorRectangle(this.GetBounds() * this.Layer!.Zoom);
            this.DrawCore(ctx);
            ctx.PopScissorRectangle();
        }
        else
        {
            this.DrawCore(ctx);
        }

        if (this.Opacity < 1f)
        {
            ctx.PopOpacity();
        }
    }

    /// <summary>
    /// Calculates the actual rendered area of the control within the GUI system.
    /// This method returns a <see cref="Rect"/> that defines the control's position and size
    /// after layout and arrangement have been performed.
    /// </summary>
    /// <returns>A <see cref="Rect"/> representing the control's bounding box in screen coordinates.</returns>
    protected abstract Rect GetBounds();

    /// <summary>
    /// Implements the core drawing logic for the control. Derived classes should override this method
    /// to render their specific visual content, such as backgrounds, borders, or child elements.
    /// </summary>
    /// <param name="ctx">The <see cref="DrawingContext"/> to use for rendering operations.</param>
    protected abstract void DrawCore(DrawingContext ctx);

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

    /// <summary>
    /// Implements the core measuring logic for the control. Derived classes should override this method
    /// to calculate their desired size based on the available space and their content.
    /// </summary>
    /// <param name="availableSize">The available size that the parent element can allocate for this control.</param>
    /// <returns>The desired size of the control, including any padding or content size.</returns>
    protected abstract Vector2 MeasureCore(Vector2 availableSize);

    /// <summary>
    /// Performs the arrange pass of the layout process. In the arrange pass, the control positions its children and computes the actual size of the control.
    /// </summary>
    /// <param name="finalRect">The final size that this object should use to arrange itself and its children.</param>
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

        if (this.Layer!.SnapToPixels)
        {
            position.X = (float)Math.Round(position.X);
            position.Y = (float)Math.Round(position.Y);
            size.X = (float)Math.Round(size.X);
            size.Y = (float)Math.Round(size.Y);
        }

        this.Position = position;
        this.ActualSize = size;
    }

    /// <summary>
    /// Implements the core arranging logic for the control. Derived classes should override this method
    /// to position and size their content within the provided final size.
    /// </summary>
    /// <param name="finalRect">
    /// The final area within the parent that this control should use to arrange itself and its children.
    /// </param>
    /// <returns>
    /// The actual size and position that the control occupies after arrangement.
    /// </returns>
    protected abstract Rect ArrangeCore(Rect finalRect);

    /// <summary>
    /// Occurs when this control is being mounted to the root <see cref="SceneGraph.Stage"/>.
    /// </summary>
    public event EventHandler? Mounted;

    /// <summary>
    /// Occurs when this control is being unmounted from the root <see cref="SceneGraph.Stage"/>.
    /// </summary>
    public event EventHandler? Unmounting;

    private IDisposable? _bindings = null;

    /// <summary>
    /// Mounts this control into the given layer, recursively mounting all children.
    /// </summary>
    /// <param name="layer">The <see cref="GuiLayer"/> to mount into.</param>
    /// <exception cref="InvalidOperationException">Thrown if the control is already mounted.</exception>
    public virtual void Mount(GuiLayer layer)
    {
        if (this.Layer != null)
        {
            throw new InvalidOperationException("The control is already mounted.");
        }

        this.IsArrangeValid = false;
        this.IsMeasureValid = false;
        this.Layer = layer;
        foreach (var child in this.VisualChildren)
        {
            child.Mount(layer);
        }

        this._bindings = this.CreateBindings();
        this.Mounted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// This method can be overridden to create bindings when the control is mounted.
    /// </summary>
    /// <returns>The created bindings.</returns>
    protected virtual IDisposable? CreateBindings()
    {
        return null;
    }

    /// <summary>
    /// Unmounts this control from the scene graph, recursively unmounting all children.
    /// </summary>
    /// <remarks>
    /// The <see cref="Layer"/> reference remains valid throughout this method and its overrides.
    /// It is cleared at the end of the base implementation after all children have been unmounted.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the control is not mounted.</exception>
    public virtual void Unmount()
    {
        if (this.Layer == null)
        {
            throw new InvalidOperationException("The control is not mounted.");
        }

        foreach (var child in this.VisualChildren)
        {
            child.Unmount();
        }

        this._bindings?.Dispose();
        this._bindings = null;
        this.Unmounting?.Invoke(this, EventArgs.Empty);
        this.Layer = null;
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

    /// <summary>
    /// Adds a visual child to the control.
    /// </summary>
    /// <param name="child">The child visual to add.</param>
    protected virtual void AddVisualChild(Visual child)
    {
        this._visualChildren.Add(child);
        child.Parent = this;
        if (this.Layer != null)
        {
            child.Mount(this.Layer);
            this.InvalidateMeasure();
        }
    }

    /// <summary>
    /// Removes a visual child from the control.
    /// </summary>
    /// <param name="child">The child visual to remove.</param>
    protected virtual void RemoveVisualChild(Visual child)
    {
        this._visualChildren.Remove(child);
        if (this.Layer != null)
        {
            child.Unmount();
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
    /// <summary>
    /// Invalidates the measure of the control and its children.
    /// </summary>
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

    /// <summary>
    /// Invalidates the arrange of the control and its children.
    /// </summary>
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

    /// <summary>
    /// Gets whether the control has been disposed.
    /// </summary>
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

    /// <summary>
    /// Finalizes an instance of the <see cref="Visual"/> class.
    /// </summary>
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
