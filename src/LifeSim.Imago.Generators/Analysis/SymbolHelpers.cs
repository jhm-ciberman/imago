using System.Linq;
using Microsoft.CodeAnalysis;

namespace LifeSim.Imago.Generators.Analysis;

/// <summary>
/// Shared helpers for Roslyn symbol lookups and C# code generation.
/// </summary>
internal static class SymbolHelpers
{
    /// <summary>
    /// Walks the type hierarchy to find a property by name.
    /// </summary>
    /// <param name="type">The type to search.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The property symbol, or null if not found.</returns>
    public static IPropertySymbol? FindProperty(ITypeSymbol type, string propertyName)
    {
        var current = type;
        while (current != null)
        {
            var prop = current.GetMembers(propertyName)
                .OfType<IPropertySymbol>()
                .FirstOrDefault();
            if (prop != null)
            {
                return prop;
            }

            current = current.BaseType;
        }

        return null;
    }

    /// <summary>
    /// Walks the type hierarchy to find a field or property by name and returns its type.
    /// </summary>
    /// <param name="type">The type to search.</param>
    /// <param name="memberName">The member name.</param>
    /// <returns>The member's type, or <see langword="null"/> if not found.</returns>
    public static ITypeSymbol? FindMemberType(ITypeSymbol type, string memberName)
    {
        var current = type;
        while (current != null)
        {
            foreach (var member in current.GetMembers(memberName))
            {
                if (member is IFieldSymbol field)
                {
                    return field.Type;
                }

                if (member is IPropertySymbol property)
                {
                    return property.Type;
                }
            }

            current = current.BaseType;
        }

        return null;
    }

    /// <summary>
    /// Walks the type hierarchy to find an attribute by fully qualified name.
    /// </summary>
    /// <param name="type">The type to search.</param>
    /// <param name="attributeFullName">The fully qualified attribute class name.</param>
    /// <returns>The attribute data, or <see langword="null"/> if not found.</returns>
    public static AttributeData? FindAttribute(ITypeSymbol type, string attributeFullName)
    {
        var current = type;
        while (current != null)
        {
            foreach (var attr in current.GetAttributes())
            {
                if (attr.AttributeClass?.ToDisplayString() == attributeFullName)
                {
                    return attr;
                }
            }

            current = current.BaseType;
        }

        return null;
    }

    /// <summary>
    /// Escapes a string for use inside a C# string literal.
    /// </summary>
    /// <param name="s">The string to escape.</param>
    /// <returns>The escaped string.</returns>
    public static string EscapeCSharpString(string s)
    {
        return s
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    /// <summary>
    /// Escapes a character for use inside a C# character literal.
    /// </summary>
    /// <param name="c">The character to escape.</param>
    /// <returns>The escaped character representation.</returns>
    public static string EscapeCSharpChar(char c)
    {
        return c switch
        {
            '\\' => "\\\\",
            '\'' => "\\'",
            '\n' => "\\n",
            '\r' => "\\r",
            '\t' => "\\t",
            '\0' => "\\0",
            _ => c.ToString(),
        };
    }
}
