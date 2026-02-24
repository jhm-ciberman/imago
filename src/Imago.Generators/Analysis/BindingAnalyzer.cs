using System;
using System.Linq;
using Imago.Generators.Diagnostics;
using Imago.Generators.Parsing;
using Microsoft.CodeAnalysis;

namespace Imago.Generators.Analysis;

/// <summary>
/// Analyzes binding expressions from template <c>bind:</c> attributes, resolving source
/// and target types via Roslyn and validating INPC implementation.
/// </summary>
internal static class BindingAnalyzer
{
    /// <summary>
    /// Analyzes a single binding expression on a template element.
    /// </summary>
    /// <param name="classType">The code-behind class type symbol.</param>
    /// <param name="elementType">The resolved type of the target element.</param>
    /// <param name="targetProperty">The property name on the target element (e.g., "Value").</param>
    /// <param name="expression">The binding expression string (e.g., "this._vm.Hunger").</param>
    /// <param name="span">The source location of the binding attribute.</param>
    /// <returns>An analyzed binding.</returns>
    public static AnalyzedBinding Analyze(
        INamedTypeSymbol classType,
        INamedTypeSymbol elementType,
        string targetProperty,
        string expression,
        SourceSpan span)
    {
        var parsed = ParseExpression(expression, span);

        // Resolve source type
        var sourceType = ResolveSourceType(classType, parsed.SourceSegments, expression, span);

        // Validate source implements INotifyPropertyChanged
        ValidateInpc(sourceType, span);

        // Validate source property exists and is readable
        ValidateSourceProperty(sourceType, parsed.PropertyName, span);

        // Validate target: try property first, then fall back to method
        var isMethodTarget = ResolveTarget(elementType, targetProperty, span);

        return new AnalyzedBinding
        {
            SourceExpression = parsed.SourceExpression,
            SourceKey = parsed.SourceKey,
            PropertyName = parsed.PropertyName,
            TargetProperty = targetProperty,
            IsMethodTarget = isMethodTarget,
            AssignExpression = expression.StartsWith("this.", StringComparison.Ordinal)
                ? expression
                : "this." + expression,
        };
    }

    /// <summary>
    /// Validates that a binding attribute does not conflict with a regular attribute.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="hasRegularAttribute">Whether the element also has a regular attribute with this name.</param>
    /// <param name="span">The source location of the binding attribute.</param>
    public static void ValidateNoConflict(string propertyName, bool hasRegularAttribute, SourceSpan span)
    {
        if (hasRegularAttribute)
        {
            throw new BindingConflictsWithAttributeException(propertyName).At(span);
        }
    }

    private static ParsedExpression ParseExpression(string expression, SourceSpan span)
    {
        var trimmed = expression.Trim();

        // Split on '.'
        var segments = trimmed.Split('.');
        if (segments.Length < 2)
        {
            throw new BindingInvalidExpressionException(expression).At(span);
        }

        var propertyName = segments[segments.Length - 1];

        // Build the source part (everything before the last segment)
        var sourceSegments = new string[segments.Length - 1];
        Array.Copy(segments, sourceSegments, segments.Length - 1);

        // Normalize: strip leading "this" for source key computation
        var keyStartIndex = 0;
        if (sourceSegments.Length > 0 && sourceSegments[0] == "this")
        {
            keyStartIndex = 1;
        }

        // Source key: the part after "this." (or the whole thing if no "this")
        string sourceKey;
        if (keyStartIndex >= sourceSegments.Length)
        {
            // Expression is "this.Property" - source is "this" itself
            sourceKey = "this";
        }
        else
        {
            sourceKey = string.Join(".", sourceSegments, keyStartIndex, sourceSegments.Length - keyStartIndex);
        }

        // Source expression: always include "this."
        string sourceExpression;
        if (sourceSegments[0] == "this")
        {
            sourceExpression = string.Join(".", sourceSegments);
        }
        else
        {
            sourceExpression = "this." + string.Join(".", sourceSegments);
        }

        return new ParsedExpression
        {
            SourceSegments = sourceSegments,
            SourceExpression = sourceExpression,
            SourceKey = sourceKey,
            PropertyName = propertyName,
        };
    }

    private static ITypeSymbol ResolveSourceType(
        INamedTypeSymbol classType,
        string[] sourceSegments,
        string expression,
        SourceSpan span)
    {
        var startIndex = 0;
        if (sourceSegments.Length > 0 && sourceSegments[0] == "this")
        {
            startIndex = 1;
        }

        // If source is just "this" (e.g., "this.MyProperty"), the source type is the class itself
        if (startIndex >= sourceSegments.Length)
        {
            return classType;
        }

        // Walk from the class type, resolving each segment as a field or property
        ITypeSymbol currentType = classType;
        for (var i = startIndex; i < sourceSegments.Length; i++)
        {
            var memberName = sourceSegments[i];
            var memberType = SymbolHelpers.FindMemberType(currentType, memberName)
                ?? throw new BindingInvalidExpressionException(expression).At(span);
            currentType = memberType;
        }

        return currentType;
    }

    private static void ValidateInpc(ITypeSymbol sourceType, SourceSpan span)
    {
        var implementsInpc = sourceType.AllInterfaces.Any(i =>
            i.ToDisplayString() == "System.ComponentModel.INotifyPropertyChanged");

        // Also check if the source type itself is INotifyPropertyChanged
        if (!implementsInpc && sourceType.ToDisplayString() != "System.ComponentModel.INotifyPropertyChanged")
        {
            throw new BindingSourceNotInpcException(sourceType.ToDisplayString()).At(span);
        }
    }

    private static void ValidateSourceProperty(ITypeSymbol sourceType, string propertyName, SourceSpan span)
    {
        if (SymbolHelpers.FindProperty(sourceType, propertyName) == null)
        {
            throw new BindingPropertyNotFoundException(propertyName, sourceType.ToDisplayString()).At(span);
        }
    }

    private static bool ResolveTarget(INamedTypeSymbol elementType, string name, SourceSpan span)
    {
        // Try property first
        var prop = SymbolHelpers.FindProperty(elementType, name);
        if (prop != null)
        {
            if (prop.SetMethod == null || prop.IsReadOnly)
            {
                throw new BindingTargetNotSettableException(name).At(span);
            }

            return false;
        }

        // Fall back to method with exactly one parameter
        var method = FindMethod(elementType, name);
        if (method != null)
        {
            return true;
        }

        throw new UnknownPropertyException(name, elementType.ToDisplayString()).At(span);
    }

    private static IMethodSymbol? FindMethod(INamedTypeSymbol type, string name)
    {
        var current = (ITypeSymbol?)type;
        while (current != null)
        {
            foreach (var member in current.GetMembers(name))
            {
                if (member is IMethodSymbol { IsStatic: false, Parameters.Length: 1 } method)
                {
                    return method;
                }
            }

            current = current.BaseType;
        }

        return null;
    }

    private struct ParsedExpression
    {
        public string[] SourceSegments;
        public string SourceExpression;
        public string SourceKey;
        public string PropertyName;
    }
}
