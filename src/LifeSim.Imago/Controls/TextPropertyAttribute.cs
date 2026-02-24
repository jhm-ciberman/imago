using System;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Specifies the property that accepts text content from XML templates.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class TextPropertyAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the property that receives text content.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextPropertyAttribute"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the text property.</param>
    public TextPropertyAttribute(string propertyName)
    {
        this.PropertyName = propertyName;
    }
}
