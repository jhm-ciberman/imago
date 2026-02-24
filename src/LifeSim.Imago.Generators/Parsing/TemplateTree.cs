using System.Collections.Generic;

namespace LifeSim.Imago.Generators.Parsing;

/// <summary>
/// The root of a parsed template, containing the AST and metadata.
/// </summary>
internal sealed class TemplateTree
{
    /// <summary>
    /// Gets or sets the root element node.
    /// </summary>
    public TemplateNode Root { get; set; } = null!;

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
    /// Gets or sets the xmlns prefix-to-namespace map.
    /// </summary>
    public XmlNamespaceMap NamespaceMap { get; set; } = null!;

    /// <summary>
    /// Gets or sets the template file path (for diagnostics).
    /// </summary>
    public string FilePath { get; set; } = "";

    /// <summary>
    /// Gets nested class templates defined via x:Class on child elements.
    /// </summary>
    public List<TemplateTree> NestedClasses { get; } = new List<TemplateTree>();
}
