using Microsoft.CodeAnalysis;

namespace Imago.Generators.Analysis;

/// <summary>
/// Detects types marked with <c>[FactoryTemplate]</c> and resolves their delegate constructor signature.
/// </summary>
internal static class FactoryTemplateAnalyzer
{
    private const string FactoryTemplateAttributeName = "Imago.Controls.FactoryTemplateAttribute";

    /// <summary>
    /// Checks whether the given type (or any base type) is marked with <c>[FactoryTemplate]</c>.
    /// For generic types, checks the original (open) definition.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is a factory template.</returns>
    public static bool IsFactoryTemplate(INamedTypeSymbol type)
    {
        var definition = type.IsGenericType ? type.OriginalDefinition : type;
        return SymbolHelpers.FindAttribute(definition, FactoryTemplateAttributeName) != null;
    }

    /// <summary>
    /// Inspects the type's constructors to find one with a delegate parameter, and extracts
    /// the delegate's parameter and return types.
    /// </summary>
    /// <param name="constructedType">The constructed (closed) generic type to inspect.</param>
    /// <returns>The resolved factory template information, or <see langword="null"/> if no suitable constructor was found.</returns>
    public static FactoryTemplateInfo? Analyze(INamedTypeSymbol constructedType)
    {
        foreach (var ctor in constructedType.Constructors)
        {
            if (ctor.IsImplicitlyDeclared)
            {
                continue;
            }

            foreach (var param in ctor.Parameters)
            {
                if (param.Type is INamedTypeSymbol paramType && paramType.DelegateInvokeMethod != null)
                {
                    var invokeMethod = paramType.DelegateInvokeMethod;

                    if (invokeMethod.Parameters.Length < 1)
                    {
                        continue;
                    }

                    return new FactoryTemplateInfo
                    {
                        ParameterTypeName = invokeMethod.Parameters[0].Type.ToDisplayString(),
                        ReturnTypeName = invokeMethod.ReturnType.ToDisplayString(),
                    };
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Checks whether any descendant node has an <c>x:Name</c> directive set.
    /// </summary>
    /// <param name="node">The root node to check (typically the factory body root).</param>
    /// <returns>The name of the first named descendant found, or <see langword="null"/>.</returns>
    public static string? FindNamedDescendant(Parsing.TemplateNode node)
    {
        if (node.Name != null)
        {
            return node.Name;
        }

        foreach (var child in node.Children)
        {
            var found = FindNamedDescendant(child);
            if (found != null)
            {
                return found;
            }
        }

        foreach (var propChildren in node.PropertyElements.Values)
        {
            foreach (var child in propChildren)
            {
                var found = FindNamedDescendant(child);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }
}
