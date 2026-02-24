using System;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Specifies the property that accepts child elements as a collection in XML templates.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class ItemsPropertyAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the property that accepts child elements.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemsPropertyAttribute"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the collection property.</param>
    public ItemsPropertyAttribute(string propertyName)
    {
        this.PropertyName = propertyName;
    }
}
