using System;

namespace Imago.Controls;

/// <summary>
/// Marks a class as a factory template for the template source generator. When an element with
/// this attribute is encountered in a template, its child elements define the factory body rather
/// than being added as children. The generator emits a private method that constructs the child
/// tree and passes it to the type's constructor as a delegate.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class FactoryTemplateAttribute : Attribute
{
}
