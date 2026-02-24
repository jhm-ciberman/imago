using System;
using LifeSim.Imago.Generators.Parsing;
using Microsoft.CodeAnalysis;

namespace LifeSim.Imago.Generators.Diagnostics;

/// <summary>
/// Base exception for template analysis errors. Each subclass represents a specific
/// diagnostic that is caught at the analysis entry point and reported to Roslyn.
/// </summary>
internal abstract class TemplateDiagnosticException : Exception
{
    private readonly DiagnosticDescriptor _descriptor;
    private readonly object[] _args;

    /// <summary>
    /// Gets the Roslyn location for this diagnostic, pointing into the template XML file.
    /// </summary>
    public Location Location { get; private set; } = Location.None;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateDiagnosticException"/> class.
    /// </summary>
    /// <param name="descriptor">The diagnostic descriptor for this error.</param>
    /// <param name="args">The message format arguments, passed to both the exception message and the Roslyn diagnostic.</param>
    protected TemplateDiagnosticException(DiagnosticDescriptor descriptor, params object[] args)
        : base(string.Format(descriptor.MessageFormat.ToString(), args))
    {
        this._descriptor = descriptor;
        this._args = args;
    }

    /// <summary>
    /// Sets the source location for this diagnostic and returns the exception for fluent throw syntax.
    /// </summary>
    /// <param name="span">The source location in the template file.</param>
    /// <returns>This exception instance.</returns>
    public TemplateDiagnosticException At(SourceSpan span)
    {
        this.Location = span.ToLocation();
        return this;
    }

    /// <summary>
    /// Creates the Roslyn diagnostic from this exception.
    /// </summary>
    /// <returns>A <see cref="Diagnostic"/> ready to report to the source production context.</returns>
    public Diagnostic CreateDiagnostic()
    {
        return Diagnostic.Create(this._descriptor, this.Location, this._args);
    }
}

// --- XML Parsing ---

internal sealed class XmlParseException(string detail)
    : TemplateDiagnosticException(DiagnosticDescriptors.XmlParseError, detail)
{
    public string Detail => detail;
}

internal sealed class CodeBehindNotFoundException(string className)
    : TemplateDiagnosticException(DiagnosticDescriptors.CodeBehindNotFound, className)
{
    public string ClassName => className;
}

// --- Type Resolution ---

internal sealed class UnknownTypeException(string elementName, string searchedLocation)
    : TemplateDiagnosticException(DiagnosticDescriptors.UnknownType, elementName, searchedLocation)
{
    public string ElementName => elementName;
    public string SearchedLocation => searchedLocation;
}

internal sealed class AmbiguousTypeException(string elementName, string firstNamespace, string secondNamespace)
    : TemplateDiagnosticException(
        DiagnosticDescriptors.AmbiguousType,
        elementName, firstNamespace, secondNamespace)
{
    public string ElementName => elementName;
    public string FirstNamespace => firstNamespace;
    public string SecondNamespace => secondNamespace;
}

// --- Property Resolution ---

internal sealed class UnknownPropertyException(string propertyName, string typeName)
    : TemplateDiagnosticException(DiagnosticDescriptors.UnknownProperty, propertyName, typeName)
{
    public string PropertyName => propertyName;
    public string TypeName => typeName;
}

internal sealed class PropertyNotSettableException(string propertyName, string typeName)
    : TemplateDiagnosticException(DiagnosticDescriptors.PropertyNotSettable, propertyName, typeName)
{
    public string PropertyName => propertyName;
    public string TypeName => typeName;
}

internal sealed class InvalidPropertyValueException(string value, string propertyName, string expectedType)
    : TemplateDiagnosticException(
        DiagnosticDescriptors.InvalidPropertyValue,
        value, propertyName, expectedType)
{
    public string Value => value;
    public string PropertyName => propertyName;
    public string ExpectedType => expectedType;
}

// --- Children & Content ---

internal sealed class ChildrenNotAllowedException(string typeName)
    : TemplateDiagnosticException(DiagnosticDescriptors.NoChildSlot, typeName)
{
    public string TypeName => typeName;
}

internal sealed class TooManyChildrenException(string typeName, int childCount)
    : TemplateDiagnosticException(DiagnosticDescriptors.TooManyChildren, typeName, childCount)
{
    public string TypeName => typeName;
    public int ChildCount => childCount;
}

internal sealed class TextContentNotAllowedException(string typeName)
    : TemplateDiagnosticException(DiagnosticDescriptors.TextContentNotAllowed, typeName)
{
    public string TypeName => typeName;
}

internal sealed class ConflictingTextContentException(string propertyName)
    : TemplateDiagnosticException(DiagnosticDescriptors.ConflictingTextContent, propertyName)
{
    public string PropertyName => propertyName;
}

internal sealed class TextContentWithChildrenException()
    : TemplateDiagnosticException(DiagnosticDescriptors.TextContentWithChildren)
{
}

// --- Misc ---

internal sealed class ArgumentsOnRootException()
    : TemplateDiagnosticException(DiagnosticDescriptors.ArgumentsOnRoot)
{
}

internal sealed class ArgumentsOnFactoryException()
    : TemplateDiagnosticException(DiagnosticDescriptors.ArgumentsOnFactory)
{
}

// --- Factory Templates ---

internal sealed class MissingTypeArgumentsException(string typeName)
    : TemplateDiagnosticException(DiagnosticDescriptors.MissingTypeArguments, typeName)
{
    public string TypeName => typeName;
}

internal sealed class UnresolvedTypeArgumentException(string typeArgumentName)
    : TemplateDiagnosticException(DiagnosticDescriptors.UnresolvedTypeArgument, typeArgumentName)
{
    public string TypeArgumentName => typeArgumentName;
}

internal sealed class FactoryChildCountException(int childCount)
    : TemplateDiagnosticException(DiagnosticDescriptors.FactoryChildCount, childCount)
{
    public int ChildCount => childCount;
}

internal sealed class NameInFactoryException(string elementName)
    : TemplateDiagnosticException(DiagnosticDescriptors.NameInFactory, elementName)
{
    public string ElementName => elementName;
}

internal sealed class NoDelegateConstructorException(string typeName)
    : TemplateDiagnosticException(DiagnosticDescriptors.NoDelegateConstructor, typeName)
{
    public string TypeName => typeName;
}

internal sealed class TypeArgumentsOnNonFactoryException(string typeName)
    : TemplateDiagnosticException(DiagnosticDescriptors.TypeArgumentsOnNonFactory, typeName)
{
    public string TypeName => typeName;
}

// --- Names ---

internal sealed class DuplicateNameException(string name)
    : TemplateDiagnosticException(DiagnosticDescriptors.DuplicateName, name)
{
    public string Name => name;
}

// --- Bindings ---

internal sealed class BindingSourceNotInpcException(string typeName)
    : TemplateDiagnosticException(DiagnosticDescriptors.BindingSourceNotInpc, typeName)
{
    public string TypeName => typeName;
}

internal sealed class BindingPropertyNotFoundException(string propertyName, string typeName)
    : TemplateDiagnosticException(DiagnosticDescriptors.BindingPropertyNotFound, propertyName, typeName)
{
    public string PropertyName => propertyName;
    public string TypeName => typeName;
}

internal sealed class BindingTargetNotSettableException(string propertyName)
    : TemplateDiagnosticException(DiagnosticDescriptors.BindingTargetNotSettable, propertyName)
{
    public string PropertyName => propertyName;
}

internal sealed class BindingConflictsWithAttributeException(string propertyName)
    : TemplateDiagnosticException(DiagnosticDescriptors.BindingConflictsWithAttribute, propertyName)
{
    public string PropertyName => propertyName;
}

internal sealed class BindingInvalidExpressionException(string expression)
    : TemplateDiagnosticException(DiagnosticDescriptors.BindingInvalidExpression, expression)
{
    public string Expression => expression;
}

internal sealed class BindingInDataTemplateException()
    : TemplateDiagnosticException(DiagnosticDescriptors.BindingInDataTemplate)
{
}
