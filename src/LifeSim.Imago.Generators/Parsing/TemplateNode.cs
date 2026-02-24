using System.Collections.Generic;

namespace LifeSim.Imago.Generators.Parsing;

/// <summary>
/// Intermediate AST node representing an XML element in the template.
/// </summary>
internal sealed class TemplateNode
{
    /// <summary>
    /// Gets or sets the local element name (e.g., "StackPanel", "TextBlock").
    /// </summary>
    public string ElementName { get; set; } = "";

    /// <summary>
    /// Gets or sets the source location of this element's opening tag.
    /// </summary>
    public SourceSpan ElementSpan { get; set; }

    /// <summary>
    /// Gets or sets the element reference name from the <c>x:Name</c> directive, if present.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets the attributes on this element (excluding xmlns and x: directives).
    /// </summary>
    public Dictionary<string, string> Attributes { get; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets the source locations for each attribute, keyed by attribute name.
    /// </summary>
    public Dictionary<string, SourceSpan> AttributeSpans { get; } = new Dictionary<string, SourceSpan>();

    /// <summary>
    /// Gets the binding attributes on this element (bind:Property -> expression).
    /// </summary>
    public Dictionary<string, string> Bindings { get; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets the source locations for each binding attribute, keyed by property name.
    /// </summary>
    public Dictionary<string, SourceSpan> BindingSpans { get; } = new Dictionary<string, SourceSpan>();

    /// <summary>
    /// Gets the child element nodes.
    /// </summary>
    public List<TemplateNode> Children { get; } = new List<TemplateNode>();

    /// <summary>
    /// Gets the property element children (e.g., Button.Content mapped as "Content" key).
    /// </summary>
    public Dictionary<string, List<TemplateNode>> PropertyElements { get; } = new Dictionary<string, List<TemplateNode>>();

    /// <summary>
    /// Gets or sets the text content from the XML element, after whitespace normalization.
    /// </summary>
    public string? TextContent { get; set; }

    /// <summary>
    /// Gets or sets the source location of the text content.
    /// </summary>
    public SourceSpan? TextContentSpan { get; set; }

    /// <summary>
    /// Gets or sets the raw C# expression for constructor arguments, if specified via x:Arguments.
    /// </summary>
    public string? ConstructorArguments { get; set; }

    /// <summary>
    /// Gets or sets the source location of the x:Arguments attribute.
    /// </summary>
    public SourceSpan? ConstructorArgumentsSpan { get; set; }

    /// <summary>
    /// Gets or sets the type argument string from x:TypeArguments (e.g., "Ground").
    /// </summary>
    public string? TypeArguments { get; set; }

    /// <summary>
    /// Gets or sets the source location of the x:TypeArguments attribute.
    /// </summary>
    public SourceSpan? TypeArgumentsSpan { get; set; }

    /// <summary>
    /// Gets or sets the nested class name from x:Class when used on a child element.
    /// </summary>
    public string? NestedClassName { get; set; }

    /// <summary>
    /// Gets or sets the source location of the x:Class attribute on a child element.
    /// </summary>
    public SourceSpan? NestedClassNameSpan { get; set; }
}
