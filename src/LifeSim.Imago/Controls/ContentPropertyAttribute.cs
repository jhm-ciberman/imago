using System;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Specifies the property that accepts a single child element in XML templates.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class ContentPropertyAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the property that accepts a single child element.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentPropertyAttribute"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the content property.</param>
    public ContentPropertyAttribute(string propertyName)
    {
        this.PropertyName = propertyName;
    }
}
