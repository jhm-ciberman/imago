using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Controls;

public abstract class Visual
{
    /// <summary>
    /// Gets or sets the name of the element.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the visibility of the control.
    /// </summary>
    public Visibility Visibility { get; set; } = Visibility.Visible;

    /// <summary>
    /// Gets the root of the control.
    /// </summary>
    private UIPage? _root;
    public UIPage? Root
    {
        get => this._root;
        private set => this._root = value;
    }


    /// <summary>
    /// Gets or sets the opacity of the control.
    /// </summary>
    public float Opacity { get; set; } = 1f;

    /// <summary>
    /// Gets an enumerable collection of the control's visual children.
    /// </summary>
    public virtual IEnumerable<Control> VisualChildren { get; } = Enumerable.Empty<Control>();

    /// <summary>
    /// Gets or sets whether the content is clipped to the control's bounds.
    /// </summary>
    public bool ClipToBounds { get; set; } = false;

    /// <summary>
    /// Gets or sets the style of the control.
    /// </summary>
    private Style? _style;

    public Style? Style
    {
        get => this._style;
        set
        {
            if (this._style != value)
            {
                this._style = value;
                value?.Apply(this);
            }
        }
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
            spriteBatcher.PushScissorRectangle(GetBounds() * this.Root!.Zoom);
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

    public virtual void OnAddedToVisualTree(UIPage page)
    {
        if (this.Root != page)
        {
            this.Root = page;
            foreach (var child in this.VisualChildren)
            {
                child.OnAddedToVisualTree(page);
            }
        }
    }

    public virtual void OnRemovedFromVisualTree(UIPage page)
    {
        if (this.Root == page)
        {
            this.Root = null;
            foreach (var child in this.VisualChildren)
            {
                child.OnRemovedFromVisualTree(page);
            }
        }
    }

    /// <summary>
    /// Finds a child control of the specified type by its name recursively.
    /// </summary>
    /// <typeparam name="T">The type of the control to find.</typeparam>
    /// <param name="name">The name of the control to find.</param>
    /// <returns>The control if found, otherwise null.</returns>
    public T? GetElementByName<T>(string name) where T : Visual
    {
        if (this.Name == name)
        {
            return (T)this;
        }

        foreach (var child in this.VisualChildren)
        {
            var result = child.GetElementByName<T>(name);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}