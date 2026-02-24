using System;
using System.Linq;
using Imago.Generators.Diagnostics;
using Imago.Generators.Parsing;
using Microsoft.CodeAnalysis;

namespace Imago.Generators.Analysis;

/// <summary>
/// Analyzes attribute values into C# expressions based on the target property type.
/// </summary>
internal static class PropertyAnalyzer
{
    /// <summary>
    /// Analyzes a single property assignment.
    /// </summary>
    /// <param name="type">The type that owns the property.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="value">The attribute value string.</param>
    /// <param name="span">The source location of the attribute.</param>
    /// <returns>An analyzed property.</returns>
    public static AnalyzedProperty Analyze(
        INamedTypeSymbol type,
        string propertyName,
        string value,
        SourceSpan span)
    {
        var propSymbol = SymbolHelpers.FindProperty(type, propertyName)
            ?? throw new UnknownPropertyException(propertyName, type.ToDisplayString()).At(span);

        if (propSymbol.SetMethod == null || propSymbol.IsReadOnly)
        {
            throw new PropertyNotSettableException(propertyName, type.ToDisplayString()).At(span);
        }

        var expression = ConvertValue(value, propSymbol.Type, propertyName, span);

        return new AnalyzedProperty
        {
            PropertyName = propertyName,
            Expression = expression,
            Span = span,
        };
    }

    private static string ConvertValue(string value, ITypeSymbol propType, string propertyName, SourceSpan span)
    {
        // {{content}} — escaped braces, produce string literal "{content}"
        if (value.StartsWith("{{", StringComparison.Ordinal) && value.EndsWith("}}", StringComparison.Ordinal))
        {
            var literal = value.Substring(2, value.Length - 4);
            return "\"" + SymbolHelpers.EscapeCSharpString("{" + literal + "}") + "\"";
        }

        // {content} — raw C# expression, always
        if (value.StartsWith("{", StringComparison.Ordinal) && value.EndsWith("}", StringComparison.Ordinal))
        {
            return value.Substring(1, value.Length - 2);
        }

        // Bare value — auto-conversion based on type
        return ConvertBareValue(value, UnwrapNullable(propType), propertyName, span);
    }

    private static string ConvertBareValue(string value, ITypeSymbol actualType, string propertyName, SourceSpan span)
    {
        // string — auto-quote
        if (actualType.SpecialType == SpecialType.System_String)
        {
            return "\"" + SymbolHelpers.EscapeCSharpString(value) + "\"";
        }

        // bool
        if (actualType.SpecialType == SpecialType.System_Boolean)
        {
            if (value == "true" || value == "false")
            {
                return value;
            }

            throw new InvalidPropertyValueException(value, propertyName, "bool").At(span);
        }

        // char
        if (actualType.SpecialType == SpecialType.System_Char)
        {
            if (value.Length == 1)
            {
                return "'" + SymbolHelpers.EscapeCSharpChar(value[0]) + "'";
            }

            throw new InvalidPropertyValueException(value, propertyName, "char").At(span);
        }

        // Numeric types (int, float, double, decimal, byte, etc.)
        if (NumericHelper.IsNumericType(actualType.SpecialType))
        {
            if (NumericHelper.TryParseLiteral(actualType.SpecialType, value, out var literal))
            {
                return literal;
            }

            if (TryMatchStaticMember(actualType, value, out var qualified))
            {
                return qualified;
            }

            throw new InvalidPropertyValueException(value, propertyName, actualType.ToDisplayString()).At(span);
        }

        // enum — qualify if member name matches, otherwise error
        if (actualType.TypeKind == TypeKind.Enum)
        {
            if (TryMatchStaticMember(actualType, value, out var qualified))
            {
                return qualified;
            }

            throw new InvalidPropertyValueException(value, propertyName, actualType.ToDisplayString()).At(span);
        }

        // IParsable<TSelf> — Type.Parse("value", CultureInfo.InvariantCulture)
        if (HasPublicIParsableParseMethod(actualType))
        {
            return actualType.ToDisplayString() + ".Parse(\""
                + SymbolHelpers.EscapeCSharpString(value)
                + "\", System.Globalization.CultureInfo.InvariantCulture)";
        }

        // Other struct/class — constructor sugar
        if (actualType.SpecialType == SpecialType.None && actualType.TypeKind != TypeKind.Enum)
        {
            return "new " + actualType.ToDisplayString() + "(" + value + ")";
        }

        return value;
    }

    private static bool TryMatchStaticMember(ITypeSymbol type, string name, out string qualifiedName)
    {
        // "NaN" → check if type has a static member "NaN" → "float.NaN"
        if (HasStaticMember(type, name))
        {
            qualifiedName = type.ToDisplayString() + "." + name;
            return true;
        }

        // "float.NaN" → strip prefix, check if "NaN" is a static member → pass through as-is
        var dotIndex = name.LastIndexOf('.');
        if (dotIndex >= 0)
        {
            var memberName = name.Substring(dotIndex + 1);
            if (HasStaticMember(type, memberName))
            {
                qualifiedName = name;
                return true;
            }
        }

        qualifiedName = "";
        return false;
    }

    private static bool HasStaticMember(ITypeSymbol type, string name)
    {
        return type.GetMembers(name)
            .Any(m => m is IFieldSymbol { IsStatic: true } or IPropertySymbol { IsStatic: true });
    }

    private static bool HasPublicIParsableParseMethod(ITypeSymbol type)
    {
        var implementsInterface = type.AllInterfaces.Any(i =>
            i.Name == "IParsable" &&
            i.ContainingNamespace?.ToDisplayString() == "System" &&
            i.TypeArguments.Length == 1
        );

        if (!implementsInterface)
        {
            return false;
        }

        // Verify Parse(string, IFormatProvider?) is publicly accessible.
        // Some BCL types implement IParsable explicitly, making Parse inaccessible
        // as a direct static call (static abstract methods can't be invoked through
        // the interface without a generic constraint).
        return type.GetMembers("Parse")
            .OfType<IMethodSymbol>()
            .Any(m =>
                m.IsStatic &&
                m.DeclaredAccessibility == Accessibility.Public &&
                m.Parameters.Length == 2 &&
                m.Parameters[0].Type.SpecialType == SpecialType.System_String &&
                m.Parameters[1].Type.Name == "IFormatProvider"
            );
    }

    private static ITypeSymbol UnwrapNullable(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType &&
            namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
        {
            return namedType.TypeArguments[0];
        }

        return type;
    }
}
