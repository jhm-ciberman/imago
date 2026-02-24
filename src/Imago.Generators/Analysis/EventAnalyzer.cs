using System;
using Imago.Generators.Parsing;
using Microsoft.CodeAnalysis;

namespace Imago.Generators.Analysis;

/// <summary>
/// Classifies and analyzes event subscriptions from template attributes.
/// </summary>
internal static class EventAnalyzer
{
    /// <summary>
    /// Checks whether a member name corresponds to an event or delegate field on the type hierarchy.
    /// </summary>
    /// <param name="type">The type to search.</param>
    /// <param name="name">The member name to look for.</param>
    /// <returns><c>true</c> if the name matches a C# event or a public delegate field.</returns>
    public static bool IsEvent(INamedTypeSymbol type, string name)
    {
        var current = type;
        while (current != null)
        {
            foreach (var member in current.GetMembers(name))
            {
                if (member is IEventSymbol)
                {
                    return true;
                }

                if (member is IFieldSymbol field && field.Type.TypeKind == TypeKind.Delegate)
                {
                    return true;
                }
            }

            current = current.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Analyzes an event attribute into an <see cref="AnalyzedEvent"/>.
    /// </summary>
    /// <param name="eventName">The event or delegate field name.</param>
    /// <param name="value">The attribute value string from the template.</param>
    /// <param name="span">The source location of the attribute.</param>
    /// <returns>An analyzed event subscription.</returns>
    public static AnalyzedEvent Analyze(string eventName, string value, SourceSpan span)
    {
        string handlerExpression;

        if (value.StartsWith("{", StringComparison.Ordinal) && value.EndsWith("}", StringComparison.Ordinal))
        {
            handlerExpression = value.Substring(1, value.Length - 2);
        }
        else
        {
            handlerExpression = "this." + value;
        }

        return new AnalyzedEvent
        {
            EventName = eventName,
            HandlerExpression = handlerExpression,
            Span = span,
        };
    }
}
