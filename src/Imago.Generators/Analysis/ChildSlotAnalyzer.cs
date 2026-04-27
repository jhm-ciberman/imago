using Microsoft.CodeAnalysis;

namespace Imago.Generators.Analysis;

/// <summary>
/// Determines how a control type accepts children and text content by reading attributes.
/// </summary>
internal static class ChildSlotAnalyzer
{
    /// <summary>
    /// Inspects the type hierarchy for <c>[ItemsProperty]</c>, <c>[ItemsMethod]</c>,
    /// or <c>[ContentProperty]</c> attributes.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>The child slot kind and property name.</returns>
    public static ChildSlotInfo Analyze(INamedTypeSymbol type)
    {
        var itemsName = GetAttributeArg(type, KnownSymbols.ItemsPropertyAttribute);
        if (itemsName != null)
        {
            return new ChildSlotInfo(ChildSlot.Items, itemsName);
        }

        var methodName = GetAttributeArg(type, KnownSymbols.ItemsMethodAttribute);
        if (methodName != null)
        {
            return new ChildSlotInfo(ChildSlot.ItemsMethod, methodName);
        }

        var contentName = GetAttributeArg(type, KnownSymbols.ContentPropertyAttribute);
        if (contentName != null)
        {
            return new ChildSlotInfo(ChildSlot.Content, contentName);
        }

        return ChildSlotInfo.None;
    }

    /// <summary>
    /// Inspects the type hierarchy for a <c>[TextProperty]</c> attribute.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>The text property name, or null if not found.</returns>
    public static string? AnalyzeTextProperty(INamedTypeSymbol type)
    {
        return GetAttributeArg(type, KnownSymbols.TextPropertyAttribute);
    }

    private static string? GetAttributeArg(INamedTypeSymbol type, string attributeFullName)
    {
        var attr = SymbolHelpers.FindAttribute(type, attributeFullName);
        return attr?.ConstructorArguments is { Length: > 0 } args
            ? args[0].Value as string
            : null;
    }
}

/// <summary>
/// The analyzed child slot information including the property name.
/// </summary>
internal readonly struct ChildSlotInfo
{
    /// <summary>
    /// No children allowed.
    /// </summary>
    public static readonly ChildSlotInfo None = new ChildSlotInfo(ChildSlot.None, null);

    /// <summary>
    /// Gets the child slot kind.
    /// </summary>
    public ChildSlot Kind { get; }

    /// <summary>
    /// Gets the property name for the child slot.
    /// </summary>
    public string? PropertyName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChildSlotInfo"/> struct.
    /// </summary>
    /// <param name="kind">The child slot kind.</param>
    /// <param name="propertyName">The property name.</param>
    public ChildSlotInfo(ChildSlot kind, string? propertyName)
    {
        this.Kind = kind;
        this.PropertyName = propertyName;
    }
}
