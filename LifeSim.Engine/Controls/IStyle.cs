using System;

namespace LifeSim.Engine.Controls;

public interface IStyle
{
    /// <summary>
    /// Gets or sets the name of the style.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets or sets the parent style. All the properties of the parent style are also available in this style.
    /// </summary>
    public IStyle? BaseStyle { get; }

    /// <summary>
    /// Applies the style to the specified control.
    /// </summary>
    /// <param name="target">The control to apply the style to.</param>
    public void Apply(object target);

    /// <summary>
    /// Returns whether the style can be applied to the specified control type.
    /// </summary>
    /// <param name="targetType">The type of the control.</param>
    /// <returns>Whether the style can be applied to the specified control type.</returns>
    public bool CanApplyTo(Type targetType);
}
