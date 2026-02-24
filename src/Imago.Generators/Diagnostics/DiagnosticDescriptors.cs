using Microsoft.CodeAnalysis;

namespace Imago.Generators.Diagnostics;

/// <summary>
/// All diagnostic descriptors for the template source generator.
/// </summary>
internal static class DiagnosticDescriptors
{
    private const string Category = "Imago.Generators";

    // --- XML Parsing ---

    /// <summary>XML parsing failure.</summary>
    public static readonly DiagnosticDescriptor XmlParseError = new DiagnosticDescriptor(
        "IMAGO0001",
        "Template parse error",
        "Template file could not be parsed: {0}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>No code-behind partial class found for template.</summary>
    public static readonly DiagnosticDescriptor CodeBehindNotFound = new DiagnosticDescriptor(
        "IMAGO0002",
        "Code-behind not found",
        "Could not find a partial class '{0}' to match this template",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // --- Type Resolution ---

    /// <summary>Unknown type in namespace.</summary>
    public static readonly DiagnosticDescriptor UnknownType = new DiagnosticDescriptor(
        "IMAGO0003",
        "Unknown type",
        "Unknown type '{0}' in namespace '{1}'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Ambiguous type found in multiple imported namespaces.</summary>
    public static readonly DiagnosticDescriptor AmbiguousType = new DiagnosticDescriptor(
        "IMAGO0004",
        "Ambiguous type",
        "Type '{0}' was found in both '{1}' and '{2}'. Use a prefixed xmlns to disambiguate",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // --- Property Resolution ---

    /// <summary>Unknown property on type.</summary>
    public static readonly DiagnosticDescriptor UnknownProperty = new DiagnosticDescriptor(
        "IMAGO0005",
        "Unknown property",
        "Unknown property '{0}' on type '{1}'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Property is not settable.</summary>
    public static readonly DiagnosticDescriptor PropertyNotSettable = new DiagnosticDescriptor(
        "IMAGO0006",
        "Property not settable",
        "Property '{0}' on type '{1}' is not settable",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Bare value cannot be converted for the target property type.</summary>
    public static readonly DiagnosticDescriptor InvalidPropertyValue = new DiagnosticDescriptor(
        "IMAGO0007",
        "Invalid property value",
        "Cannot convert '{0}' to '{2}' for property '{1}'. Use {{expression}} syntax for C# expressions",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // --- Children & Content ---

    /// <summary>Type cannot have children.</summary>
    public static readonly DiagnosticDescriptor NoChildSlot = new DiagnosticDescriptor(
        "IMAGO0008",
        "Cannot have children",
        "Type '{0}' cannot have children (no Items or Content property)",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Too many children for Content slot.</summary>
    public static readonly DiagnosticDescriptor TooManyChildren = new DiagnosticDescriptor(
        "IMAGO0009",
        "Too many children",
        "Type '{0}' can only have one child (Content), but {1} were provided",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Text content used on a type without [TextProperty].</summary>
    public static readonly DiagnosticDescriptor TextContentNotAllowed = new DiagnosticDescriptor(
        "IMAGO0010",
        "Text content not allowed",
        "Type '{0}' does not accept text content (no [TextProperty] attribute)",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Property set both as XML attribute and as text content.</summary>
    public static readonly DiagnosticDescriptor ConflictingTextContent = new DiagnosticDescriptor(
        "IMAGO0011",
        "Conflicting text content",
        "Property '{0}' is set both as an XML attribute and as text content",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Element has both text content and child elements.</summary>
    public static readonly DiagnosticDescriptor TextContentWithChildren = new DiagnosticDescriptor(
        "IMAGO0012",
        "Text content with children",
        "Element cannot have both text content and child elements",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // --- Misc Warnings ---

    /// <summary>x:Arguments used on root element.</summary>
    public static readonly DiagnosticDescriptor ArgumentsOnRoot = new DiagnosticDescriptor(
        "IMAGO0013",
        "x:Arguments on root element",
        "x:Arguments is not valid on the root element (the root element represents 'this')",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>x:Arguments used on a factory template.</summary>
    public static readonly DiagnosticDescriptor ArgumentsOnFactory = new DiagnosticDescriptor(
        "IMAGO0014",
        "x:Arguments on factory template",
        "x:Arguments is not valid on factory templates",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // --- Factory Templates ---

    /// <summary>Factory template missing x:TypeArguments.</summary>
    public static readonly DiagnosticDescriptor MissingTypeArguments = new DiagnosticDescriptor(
        "IMAGO0015",
        "Missing x:TypeArguments",
        "Factory template '{0}' requires x:TypeArguments",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Type argument could not be resolved.</summary>
    public static readonly DiagnosticDescriptor UnresolvedTypeArgument = new DiagnosticDescriptor(
        "IMAGO0016",
        "Unresolved type argument",
        "Could not resolve type argument '{0}'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Factory template has wrong number of children.</summary>
    public static readonly DiagnosticDescriptor FactoryChildCount = new DiagnosticDescriptor(
        "IMAGO0017",
        "Factory template child count",
        "Factory template must have exactly one child element, but {0} were provided",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>x:Name directive inside factory template body.</summary>
    public static readonly DiagnosticDescriptor NameInFactory = new DiagnosticDescriptor(
        "IMAGO0018",
        "x:Name in factory template",
        "x:Name is not supported inside factory templates (found '{0}')",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Factory template type has no constructor with a delegate parameter.</summary>
    public static readonly DiagnosticDescriptor NoDelegateConstructor = new DiagnosticDescriptor(
        "IMAGO0019",
        "No delegate constructor",
        "Type '{0}' has no constructor accepting a delegate parameter",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>x:TypeArguments used on a non-factory-template type.</summary>
    public static readonly DiagnosticDescriptor TypeArgumentsOnNonFactory = new DiagnosticDescriptor(
        "IMAGO0020",
        "x:TypeArguments on non-factory type",
        "x:TypeArguments is only valid on types marked with [FactoryTemplate], but '{0}' is not",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // --- Names ---

    /// <summary>Duplicate x:Name in the same template.</summary>
    public static readonly DiagnosticDescriptor DuplicateName = new DiagnosticDescriptor(
        "IMAGO0021",
        "Duplicate x:Name",
        "Duplicate x:Name '{0}' in template",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // --- Bindings ---

    /// <summary>Binding source does not implement INotifyPropertyChanged.</summary>
    public static readonly DiagnosticDescriptor BindingSourceNotInpc = new DiagnosticDescriptor(
        "IMAGO0030",
        "Binding source not INPC",
        "Binding source type '{0}' does not implement INotifyPropertyChanged",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Binding property not found on source type.</summary>
    public static readonly DiagnosticDescriptor BindingPropertyNotFound = new DiagnosticDescriptor(
        "IMAGO0031",
        "Binding property not found",
        "Binding property '{0}' not found on source type '{1}'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Binding target property is not settable.</summary>
    public static readonly DiagnosticDescriptor BindingTargetNotSettable = new DiagnosticDescriptor(
        "IMAGO0032",
        "Binding target not settable",
        "Binding target property '{0}' is not settable",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Both regular attribute and binding used on same property.</summary>
    public static readonly DiagnosticDescriptor BindingConflictsWithAttribute = new DiagnosticDescriptor(
        "IMAGO0033",
        "Binding conflicts with attribute",
        "Cannot use both regular attribute and binding on same property '{0}'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Binding expression is not a valid member access.</summary>
    public static readonly DiagnosticDescriptor BindingInvalidExpression = new DiagnosticDescriptor(
        "IMAGO0034",
        "Invalid binding expression",
        "Binding expression must be a member access (e.g., this._vm.Property), got '{0}'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Binding used inside a DataTemplate body.</summary>
    public static readonly DiagnosticDescriptor BindingInDataTemplate = new DiagnosticDescriptor(
        "IMAGO0035",
        "Binding in DataTemplate",
        "Bindings are not supported inside DataTemplate bodies",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // --- Internal ---

    /// <summary>Unexpected internal generator error.</summary>
    public static readonly DiagnosticDescriptor InternalError = new DiagnosticDescriptor(
        "IMAGO9999",
        "Internal generator error",
        "Template generator failed for '{0}': {1}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
