using System;

namespace Imago.Controls;

/// <summary>
/// Specifies the method used to add child elements in XML templates.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class ItemsMethodAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the method that adds child elements.
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemsMethodAttribute"/> class.
    /// </summary>
    /// <param name="methodName">The name of the method used to add children.</param>
    public ItemsMethodAttribute(string methodName)
    {
        this.MethodName = methodName;
    }
}
