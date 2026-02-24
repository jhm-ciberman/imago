using System.Collections.Generic;
using Imago.Generators.Parsing;

namespace Imago.Generators.Analysis;

/// <summary>
/// The output of semantic analysis: a fully resolved template ready for code emission.
/// </summary>
internal sealed class AnalyzedTree
{
    /// <summary>
    /// Gets or sets the root element node.
    /// </summary>
    public AnalyzedNode Root { get; set; } = null!;

    /// <summary>
    /// Gets or sets the fully qualified class name, resolved from the code-behind partial class.
    /// </summary>
    public string ClassName { get; set; } = "";

    /// <summary>
    /// Gets the namespace extracted from the fully qualified class name.
    /// </summary>
    public string Namespace => this.ClassName.Substring(0, this.ClassName.LastIndexOf('.'));

    /// <summary>
    /// Gets the short class name (without namespace) extracted from the fully qualified class name.
    /// </summary>
    public string ShortClassName => this.ClassName.Substring(this.ClassName.LastIndexOf('.') + 1);

    /// <summary>
    /// Gets or sets the namespace map for generating using directives.
    /// </summary>
    public XmlNamespaceMap NamespaceMap { get; set; } = null!;

    /// <summary>
    /// Gets or sets the template file path for <c>#line</c> directives.
    /// </summary>
    public string FilePath { get; set; } = "";

    /// <summary>
    /// Gets analyzed nested class definitions declared via x:Class on child elements.
    /// </summary>
    public List<AnalyzedTree> NestedClasses { get; } = new List<AnalyzedTree>();
}

/// <summary>
/// A resolved element node ready for code emission.
/// </summary>
internal sealed class AnalyzedNode
{
    /// <summary>
    /// Gets or sets the element reference name from the <c>x:Name</c> directive, if present.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets whether the resolved type has a settable <c>Name</c> property.
    /// When true, <c>x:Name</c> also assigns the value to the control's <c>Name</c> property.
    /// </summary>
    public bool HasNameProperty { get; set; }

    /// <summary>
    /// Gets the generated field name (e.g., "_saveButton") derived from <c>x:Name</c>,
    /// or <see langword="null"/> if no reference name was specified.
    /// </summary>
    public string? FieldName => this.Name != null
            ? "_" + char.ToLowerInvariant(this.Name[0]) + this.Name.Substring(1)
            : null;

    /// <summary>
    /// Gets or sets the source location of this element's opening tag.
    /// </summary>
    public SourceSpan ElementSpan { get; set; }

    /// <summary>
    /// Gets or sets the fully qualified C# type name.
    /// </summary>
    public string ResolvedTypeName { get; set; } = "";

    /// <summary>
    /// Gets or sets the raw C# expression for constructor arguments.
    /// </summary>
    public string? ConstructorArguments { get; set; }

    /// <summary>
    /// Gets or sets the source location of the x:Arguments attribute.
    /// </summary>
    public SourceSpan? ConstructorArgumentsSpan { get; set; }

    /// <summary>
    /// Gets the resolved property assignments.
    /// </summary>
    public List<AnalyzedProperty> Properties { get; } = new List<AnalyzedProperty>();

    /// <summary>
    /// Gets the resolved event subscriptions.
    /// </summary>
    public List<AnalyzedEvent> Events { get; } = new List<AnalyzedEvent>();

    /// <summary>
    /// Gets the resolved template bindings.
    /// </summary>
    public List<AnalyzedBinding> Bindings { get; } = new List<AnalyzedBinding>();

    /// <summary>
    /// Gets the child element nodes.
    /// </summary>
    public List<AnalyzedNode> Children { get; } = new List<AnalyzedNode>();

    /// <summary>
    /// Gets the property element children (e.g., Button.Tooltip mapped as "Tooltip" key).
    /// </summary>
    public Dictionary<string, List<AnalyzedNode>> PropertyElements { get; } = new Dictionary<string, List<AnalyzedNode>>();

    /// <summary>
    /// Gets or sets how children are added to this control.
    /// </summary>
    public ChildSlot ChildSlot { get; set; }

    /// <summary>
    /// Gets or sets the property name for the child slot (e.g., "Children" or "Content").
    /// </summary>
    public string? ChildSlotPropertyName { get; set; }

    /// <summary>
    /// Gets or sets the text content from the XML element, after whitespace normalization.
    /// </summary>
    public string? TextContent { get; set; }

    /// <summary>
    /// Gets or sets the text property name resolved from a <c>[TextProperty]</c> attribute.
    /// </summary>
    public string? TextPropertyName { get; set; }

    /// <summary>
    /// Gets or sets whether this node is a factory template (marked with <c>[FactoryTemplate]</c>).
    /// </summary>
    public bool IsFactoryTemplate { get; set; }

    /// <summary>
    /// Gets or sets the resolved factory template information, if this node is a factory template.
    /// </summary>
    public FactoryTemplateInfo? FactoryInfo { get; set; }

    /// <summary>
    /// Gets or sets the generated variable name (e.g., "__e0") assigned during emission.
    /// </summary>
    public string VariableName { get; set; } = null!;

    /// <summary>
    /// Collects all named descendant nodes in this subtree.
    /// </summary>
    /// <returns>A list of nodes that have an <c>x:Name</c> directive.</returns>
    public List<AnalyzedNode> GetNamedDescendants()
    {
        var result = new List<AnalyzedNode>();
        CollectNamed(this, result);
        return result;
    }

    private static void CollectNamed(AnalyzedNode node, List<AnalyzedNode> result)
    {
        if (node.Name != null)
        {
            result.Add(node);
        }

        foreach (var child in node.Children)
        {
            CollectNamed(child, result);
        }

        foreach (var propChildren in node.PropertyElements.Values)
        {
            foreach (var child in propChildren)
            {
                // Factory template children are emitted in a separate method scope,
                // so their named descendants should not become class fields.
                if (child.IsFactoryTemplate)
                {
                    continue;
                }

                CollectNamed(child, result);
            }
        }
    }
}

/// <summary>
/// A resolved property assignment ready for code emission.
/// </summary>
internal sealed class AnalyzedProperty
{
    /// <summary>
    /// Gets or sets the property name as it appears on the type.
    /// </summary>
    public string PropertyName { get; set; } = "";

    /// <summary>
    /// Gets or sets the C# expression string for the property value.
    /// </summary>
    public string Expression { get; set; } = "";

    /// <summary>
    /// Gets or sets the source location of the attribute in the template file.
    /// </summary>
    public SourceSpan Span { get; set; }
}

/// <summary>
/// A resolved event subscription ready for code emission.
/// </summary>
internal sealed class AnalyzedEvent
{
    /// <summary>
    /// Gets or sets the event or delegate field name as it appears on the type.
    /// </summary>
    public string EventName { get; set; } = "";

    /// <summary>
    /// Gets or sets the C# expression for the handler (e.g., "this.OkButton_Click").
    /// </summary>
    public string HandlerExpression { get; set; } = "";

    /// <summary>
    /// Gets or sets the source location of the attribute in the template file.
    /// </summary>
    public SourceSpan Span { get; set; }
}

/// <summary>
/// Describes how children are added to a parent control.
/// </summary>
internal enum ChildSlot
{
    /// <summary>No children allowed.</summary>
    None,

    /// <summary>Children added via Items.Add().</summary>
    Items,

    /// <summary>Single child assigned via Content property.</summary>
    Content,
}

/// <summary>
/// Resolved information about a factory template's delegate signature.
/// </summary>
internal sealed class FactoryTemplateInfo
{
    /// <summary>
    /// Gets or sets the fully qualified type name of the factory parameter (e.g., the data item type).
    /// </summary>
    public string ParameterTypeName { get; set; } = "";

    /// <summary>
    /// Gets or sets the fully qualified type name of the factory return type.
    /// </summary>
    public string ReturnTypeName { get; set; } = "";

    /// <summary>
    /// Gets the parameter name used in the generated factory method.
    /// </summary>
    public string ParameterName => "item";
}
