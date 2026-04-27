using System.Collections.Generic;
using System.Linq;
using Imago.Generators.Diagnostics;
using Imago.Generators.Parsing;
using Microsoft.CodeAnalysis;

namespace Imago.Generators.Analysis;

/// <summary>
/// Performs semantic analysis on a parsed template tree, producing a fully resolved
/// <see cref="AnalyzedTree"/> ready for code emission.
/// </summary>
internal static class TemplateAnalyzer
{
    /// <summary>
    /// Analyzes a parsed template tree against the compilation.
    /// </summary>
    /// <param name="tree">The parsed template tree.</param>
    /// <param name="compilation">The Roslyn compilation for type lookup.</param>
    /// <returns>The analyzed tree.</returns>
    public static AnalyzedTree Analyze(TemplateTree tree, Compilation compilation)
    {
        var className = ResolveClassName(tree.ClassName, tree.Root.ElementName, compilation, tree.Root.ElementSpan);
        var classType = compilation.GetTypeByMetadataName(className)!;
        var root = AnalyzeNode(tree.Root, tree.NamespaceMap, compilation, classType, isRoot: true, insideFactory: false);

        ValidateUniqueNames(root);

        var implementsIMountable = SymbolHelpers.ImplementsInterface(classType, KnownSymbols.MountableInterface);

        if (HasAnyBindings(root) && !implementsIMountable)
        {
            throw new BindingRequiresIMountableException(className).At(tree.Root.ElementSpan);
        }

        var result = new AnalyzedTree
        {
            Root = root,
            ClassName = className,
            NamespaceMap = tree.NamespaceMap,
            FilePath = tree.FilePath,
            ImplementsIMountable = implementsIMountable,
        };

        // Analyze nested class definitions
        foreach (var nested in tree.NestedClasses)
        {
            var nestedClassName = ResolveNestedClassName(
                className, nested.ClassName, nested.Root.ElementName, compilation, nested.Root.ElementSpan);
            // GetTypeByMetadataName requires '+' for nested types
            var nestedClassType = compilation.GetTypeByMetadataName(className + "+" + nested.ClassName)
                ?? throw new CodeBehindNotFoundException(nested.ClassName).At(nested.Root.ElementSpan);
            var nestedRoot = AnalyzeNode(nested.Root, nested.NamespaceMap, compilation, nestedClassType, isRoot: true, insideFactory: false);

            ValidateUniqueNames(nestedRoot);

            result.NestedClasses.Add(new AnalyzedTree
            {
                Root = nestedRoot,
                ClassName = nestedClassName,
                NamespaceMap = nested.NamespaceMap,
                FilePath = nested.FilePath,
            });
        }

        return result;
    }

    private static AnalyzedNode AnalyzeNode(
        TemplateNode parsed,
        XmlNamespaceMap namespaceMap,
        Compilation compilation,
        INamedTypeSymbol? classType,
        bool isRoot,
        bool insideFactory)
    {
        // Validate: x:Arguments on root is not valid
        if (isRoot && parsed.ConstructorArguments != null)
        {
            throw new ArgumentsOnRootException().At(parsed.ConstructorArgumentsSpan ?? parsed.ElementSpan);
        }

        // Resolve type, handling generic types when x:TypeArguments is present
        var typeSymbol = ResolveType(parsed.ElementName, namespaceMap, compilation, parsed.ElementSpan, parsed.TypeArguments);

        // For root elements, use the code-behind class for property resolution since
        // properties and events may be defined on the partial class, not the base type.
        var effectiveType = isRoot && classType != null ? classType : typeSymbol;

        // Detect factory template
        var isFactory = FactoryTemplateAnalyzer.IsFactoryTemplate(typeSymbol);

        if (parsed.TypeArguments != null && !isFactory)
        {
            throw new TypeArgumentsOnNonFactoryException(parsed.ElementName)
                .At(parsed.TypeArgumentsSpan ?? parsed.ElementSpan);
        }

        FactoryTemplateInfo? factoryInfo = null;

        if (isFactory)
        {
            if (parsed.TypeArguments == null)
            {
                throw new MissingTypeArgumentsException(parsed.ElementName).At(parsed.ElementSpan);
            }

            // Resolve the type argument and construct the closed generic
            var typeArgSymbol = ResolveType(parsed.TypeArguments, namespaceMap, compilation,
                parsed.TypeArgumentsSpan ?? parsed.ElementSpan, typeArguments: null);
            var constructedType = typeSymbol.Construct(typeArgSymbol);
            typeSymbol = constructedType;

            // Inspect the delegate constructor
            factoryInfo = FactoryTemplateAnalyzer.Analyze(constructedType)
                ?? throw new NoDelegateConstructorException(parsed.ElementName).At(parsed.ElementSpan);

            // Validate: no x:Arguments on factory templates
            if (parsed.ConstructorArguments != null)
            {
                throw new ArgumentsOnFactoryException().At(parsed.ConstructorArgumentsSpan ?? parsed.ElementSpan);
            }

            // Validate child count
            if (parsed.Children.Count != 1)
            {
                throw new FactoryChildCountException(parsed.Children.Count).At(parsed.ElementSpan);
            }

            // Validate no named elements inside factory body
            var namedDescendant = FactoryTemplateAnalyzer.FindNamedDescendant(parsed.Children[0]);
            if (namedDescendant != null)
            {
                throw new NameInFactoryException(namedDescendant).At(parsed.ElementSpan);
            }
        }

        var hasNameProperty = parsed.Name != null
            && SymbolHelpers.FindProperty(typeSymbol, "Name") is { SetMethod: not null, IsReadOnly: false };

        var node = new AnalyzedNode
        {
            Name = parsed.Name,
            HasNameProperty = hasNameProperty,
            ElementSpan = parsed.ElementSpan,
            ResolvedTypeName = typeSymbol.ToDisplayString(),
            ConstructorArguments = isRoot ? null : parsed.ConstructorArguments,
            ConstructorArgumentsSpan = isRoot ? null : parsed.ConstructorArgumentsSpan,
            IsFactoryTemplate = isFactory,
            FactoryInfo = factoryInfo,
        };

        // Classify and analyze attributes as events or properties
        foreach (var kvp in parsed.Attributes)
        {
            parsed.AttributeSpans.TryGetValue(kvp.Key, out var attrSpan);

            if (EventAnalyzer.IsEvent(effectiveType, kvp.Key))
            {
                node.Events.Add(EventAnalyzer.Analyze(kvp.Key, kvp.Value, attrSpan));
            }
            else
            {
                node.Properties.Add(PropertyAnalyzer.Analyze(effectiveType, kvp.Key, kvp.Value, attrSpan));
            }
        }

        // Analyze binding attributes
        foreach (var kvp in parsed.Bindings)
        {
            parsed.BindingSpans.TryGetValue(kvp.Key, out var bindSpan);

            // Validate: no bindings inside factory template bodies
            if (insideFactory)
            {
                throw new BindingInDataTemplateException().At(bindSpan);
            }

            // Validate: no overlap with regular attributes
            BindingAnalyzer.ValidateNoConflict(kvp.Key, parsed.Attributes.ContainsKey(kvp.Key), bindSpan);

            node.Bindings.Add(BindingAnalyzer.Analyze(classType!, effectiveType, kvp.Key, kvp.Value, bindSpan));
        }

        // Determine child slot (factory templates handle children specially)
        if (!isFactory)
        {
            var childSlotInfo = ChildSlotAnalyzer.Analyze(typeSymbol);
            node.ChildSlot = childSlotInfo.Kind;
            node.ChildSlotPropertyName = childSlotInfo.PropertyName;
        }

        // Determine text property
        node.TextPropertyName = ChildSlotAnalyzer.AnalyzeTextProperty(typeSymbol);

        // Validate text content
        if (parsed.TextContent != null)
        {
            var textSpan = parsed.TextContentSpan ?? parsed.ElementSpan;

            if (node.TextPropertyName == null)
            {
                throw new TextContentNotAllowedException(node.ResolvedTypeName).At(textSpan);
            }

            if (parsed.Attributes.ContainsKey(node.TextPropertyName))
            {
                throw new ConflictingTextContentException(node.TextPropertyName).At(textSpan);
            }

            if (parsed.Children.Count > 0)
            {
                throw new TextContentWithChildrenException().At(textSpan);
            }

            node.TextContent = parsed.TextContent;
        }

        // Validate children against child slot (skip for factory templates, already validated)
        if (!isFactory)
        {
            if (parsed.Children.Count > 0 && node.ChildSlot == ChildSlot.None)
            {
                throw new ChildrenNotAllowedException(node.ResolvedTypeName).At(parsed.ElementSpan);
            }

            if (node.ChildSlot == ChildSlot.Content && parsed.Children.Count > 1)
            {
                throw new TooManyChildrenException(node.ResolvedTypeName, parsed.Children.Count).At(parsed.ElementSpan);
            }
        }

        // Analyze property elements
        foreach (var kvp in parsed.PropertyElements)
        {
            var propertyName = kvp.Key;
            var parsedChildren = kvp.Value;

            var propSymbol = SymbolHelpers.FindProperty(effectiveType, propertyName)
                ?? throw new UnknownPropertyException(propertyName, node.ResolvedTypeName).At(parsed.ElementSpan);

            var propElement = new AnalyzedPropertyElement
            {
                IsDataTemplateSlot = IsDataTemplateType(propSymbol.Type),
            };

            foreach (var child in parsedChildren)
            {
                propElement.Children.Add(AnalyzeNode(child, namespaceMap, compilation, classType, isRoot: false, insideFactory));
            }

            node.PropertyElements[propertyName] = propElement;
        }

        // Recursively analyze children (factory template body children are "inside factory")
        var childInsideFactory = insideFactory || isFactory;
        foreach (var child in parsed.Children)
        {
            node.Children.Add(AnalyzeNode(child, namespaceMap, compilation, classType, isRoot: false, childInsideFactory));
        }

        return node;
    }

    private static INamedTypeSymbol ResolveType(
        string elementName,
        XmlNamespaceMap namespaceMap,
        Compilation compilation,
        SourceSpan span,
        string? typeArguments)
    {
        // Check aliases first
        if (namespaceMap.TryResolveAlias(elementName, out var aliasedName))
        {
            return compilation.GetTypeByMetadataName(aliasedName)
                ?? throw new UnknownTypeException(elementName, aliasedName).At(span);
        }

        var candidates = namespaceMap.GetCandidateNames(elementName);

        INamedTypeSymbol? resolved = null;

        foreach (var candidate in candidates)
        {
            var symbol = compilation.GetTypeByMetadataName(candidate);
            if (symbol != null)
            {
                if (resolved != null)
                {
                    throw new AmbiguousTypeException(
                        elementName,
                        resolved.ContainingNamespace.ToDisplayString(),
                        symbol.ContainingNamespace.ToDisplayString()
                    ).At(span);
                }

                resolved = symbol;
            }
        }

        // If not found and type arguments are present, retry with generic arity suffix
        if (resolved == null && typeArguments != null)
        {
            var arity = typeArguments.Split(',').Length;

            foreach (var candidate in candidates)
            {
                var symbol = compilation.GetTypeByMetadataName(candidate + "`" + arity);
                if (symbol != null)
                {
                    if (resolved != null)
                    {
                        throw new AmbiguousTypeException(
                            elementName,
                            resolved.ContainingNamespace.ToDisplayString(),
                            symbol.ContainingNamespace.ToDisplayString()
                        ).At(span);
                    }

                    resolved = symbol;
                }
            }
        }

        return resolved
            ?? throw new UnknownTypeException(elementName, "(imported namespaces)").At(span);
    }

    private static string ResolveNestedClassName(
        string outerClassName,
        string shortNestedName,
        string rootElementName,
        Compilation compilation,
        SourceSpan span)
    {
        // Find the outer class type symbol
        var outerType = compilation.GetTypeByMetadataName(outerClassName);
        if (outerType == null)
        {
            throw new CodeBehindNotFoundException(shortNestedName).At(span);
        }

        // Look for the nested type inside the outer class
        foreach (var member in outerType.GetTypeMembers(shortNestedName))
        {
            if (member.BaseType?.Name == rootElementName)
            {
                return member.ToDisplayString();
            }
        }

        // If only one match by name, use it regardless of base type
        var candidates = outerType.GetTypeMembers(shortNestedName);
        if (candidates.Length == 1)
        {
            return candidates[0].ToDisplayString();
        }

        throw new CodeBehindNotFoundException(shortNestedName).At(span);
    }

    private static string ResolveClassName(
        string shortClassName,
        string rootElementName,
        Compilation compilation,
        SourceSpan span)
    {
        var candidates = compilation.GetSymbolsWithName(shortClassName, SymbolFilter.Type)
            .OfType<INamedTypeSymbol>()
            .ToList();

        if (candidates.Count == 1)
        {
            return candidates[0].ToDisplayString();
        }

        // Disambiguate by matching the root element to the base type
        if (candidates.Count > 1)
        {
            var match = candidates.FirstOrDefault(s => s.BaseType?.Name == rootElementName);
            if (match != null)
            {
                return match.ToDisplayString();
            }
        }

        throw new CodeBehindNotFoundException(shortClassName).At(span);
    }

    private static bool IsDataTemplateType(ITypeSymbol type)
    {
        var unannotated = type.WithNullableAnnotation(NullableAnnotation.None);

        if (unannotated.TypeKind == TypeKind.Interface && unannotated.ToDisplayString() == KnownSymbols.DataTemplateInterface)
        {
            return true;
        }

        return SymbolHelpers.ImplementsInterface(unannotated, KnownSymbols.DataTemplateInterface);
    }

    private static bool HasAnyBindings(AnalyzedNode node)
    {
        if (node.Bindings.Count > 0)
        {
            return true;
        }

        foreach (var child in node.Children)
        {
            if (HasAnyBindings(child))
            {
                return true;
            }
        }

        foreach (var propElement in node.PropertyElements.Values)
        {
            foreach (var child in propElement.Children)
            {
                if (HasAnyBindings(child))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void ValidateUniqueNames(AnalyzedNode root)
    {
        var seen = new HashSet<string>();

        foreach (var node in root.GetNamedDescendants())
        {
            if (!seen.Add(node.Name!))
            {
                throw new DuplicateNameException(node.Name!).At(node.ElementSpan);
            }
        }
    }
}
