using System;

namespace Imago.Controls;

public class Style<T> : IStyle
{
    private readonly Action<T> _applyCore;

    /// <summary>
    /// Gets or sets the name of the style.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the parent style. All the properties of the parent style are also available in this style.
    /// </summary>
    public IStyle? BaseStyle { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Style{T}"/> class.
    /// </summary>
    /// <param name="name">The name of the style.</param>
    /// <param name="baseStyle">The parent style.</param>
    /// <param name="action">The action to apply to the target object.</param>
    public Style(string? name = null, IStyle? baseStyle = null, Action<T>? action = null)
    {
        this.Name = name;
        this.BaseStyle = baseStyle;
        this._applyCore = action ?? (o => { });
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Style{T}"/> class.
    /// </summary>
    /// <param name="baseStyle">The parent style.</param>
    /// <param name="action">The action to apply to the target object.</param>
    public Style(IStyle? baseStyle = null, Action<T>? action = null)
        : this(null, baseStyle, action) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Style{T}"/> class.
    /// </summary>
    /// <param name="name">The name of the style.</param>
    /// <param name="action">The action to apply to the target object.</param>
    public Style(string? name = null, Action<T>? action = null)
        : this(name, null, action) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Style{T}"/> class.
    /// </summary>
    /// <param name="action">The action to apply to the target object.</param>
    public Style(Action<T>? action = null)
        : this(null, null, action) { }

    /// <summary>
    /// Applies the style to the specified object.
    /// </summary>
    /// <param name="target">The object to apply the style to.</param>
    public void Apply(T target)
    {
        this.BaseStyle?.Apply(target!);

        this._applyCore(target);
    }

    /// <summary>
    /// Applies the style to the specified object.
    /// </summary>
    /// <param name="target">The object to apply the style to.</param>
    /// <exception cref="ArgumentException">Thrown if the target object is not of the correct type.</exception>
    public void Apply(object target)
    {
        if (target is T typedTarget)
        {
            this.Apply(typedTarget);
        }
        else
        {
            throw new ArgumentException($"The target object is not of the correct type. Expected type: {typeof(T).FullName} but was: {target.GetType().FullName}");
        }
    }

    /// <summary>
    /// Returns whether the style can be applied to the specified control type.
    /// </summary>
    /// <param name="targetType">The type of the control.</param>
    /// <returns>Whether the style can be applied to the specified control type.</returns>
    public bool CanApplyTo(Type targetType)
    {
        // Any type that extends T is valid.
        return typeof(T).IsAssignableFrom(targetType);
    }
}
