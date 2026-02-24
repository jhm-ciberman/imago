using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using LifeSim.Imago.Generators.Diagnostics;

namespace LifeSim.Imago.Generators.Parsing;

/// <summary>
/// Parses XML template strings into <see cref="TemplateTree"/> AST structures.
/// </summary>
internal static class TemplateParser
{
    private static readonly XNamespace _directivesNs = "urn:imago:directives";
    private static readonly XNamespace _bindingsNs = "urn:imago:bindings";

    /// <summary>
    /// Parses an XML template string into a <see cref="TemplateTree"/>.
    /// </summary>
    /// <param name="xml">The XML template content.</param>
    /// <param name="filePath">The file path for diagnostics.</param>
    /// <returns>The parsed template tree.</returns>
    public static TemplateTree Parse(string xml, string filePath)
    {
        XDocument doc;
        try
        {
            doc = XDocument.Parse(xml, LoadOptions.SetLineInfo);
        }
        catch (Exception ex)
        {
            throw new XmlParseException(ex.Message);
        }

        var root = doc.Root;
        if (root == null)
        {
            throw new XmlParseException("Empty XML document");
        }

        // Derive short class name from file path (e.g. "PauseModeGui" from "PauseModeGui.template.xml")
        var fileName = Path.GetFileName(filePath);
        var className = fileName.EndsWith(".template.xml", StringComparison.OrdinalIgnoreCase)
            ? fileName.Substring(0, fileName.Length - ".template.xml".Length)
            : Path.GetFileNameWithoutExtension(fileName);

        // Build namespace map from <?using ...?> processing instructions
        var namespaceMap = new XmlNamespaceMap();

        foreach (var pi in doc.Nodes().OfType<XProcessingInstruction>())
        {
            if (pi.Target == "using")
            {
                var data = pi.Data.Trim();
                if (data.Length == 0)
                {
                    continue;
                }

                var equalsIndex = data.IndexOf('=');
                if (equalsIndex > 0)
                {
                    var alias = data.Substring(0, equalsIndex).Trim();
                    var fqn = data.Substring(equalsIndex + 1).Trim();
                    if (alias.Length > 0 && fqn.Length > 0)
                    {
                        namespaceMap.AddAlias(alias, fqn);
                    }
                }
                else
                {
                    namespaceMap.AddUsing(data);
                }
            }
        }

        // Parse element tree
        var rootNode = ParseElement(root, filePath);

        var tree = new TemplateTree
        {
            Root = rootNode,
            ClassName = className,
            NamespaceMap = namespaceMap,
            FilePath = filePath,
        };

        // Extract nested class definitions (elements with x:Class) from the entire tree
        ExtractNestedClasses(rootNode, tree.NestedClasses, namespaceMap, filePath);

        return tree;
    }

    private static void ExtractNestedClasses(
        TemplateNode node,
        List<TemplateTree> nestedClasses,
        XmlNamespaceMap namespaceMap,
        string filePath)
    {
        ExtractFromList(node.Children, nestedClasses, namespaceMap, filePath);

        foreach (var propChildren in node.PropertyElements.Values)
        {
            ExtractFromList(propChildren, nestedClasses, namespaceMap, filePath);
        }
    }

    private static void ExtractFromList(
        List<TemplateNode> children,
        List<TemplateTree> nestedClasses,
        XmlNamespaceMap namespaceMap,
        string filePath)
    {
        for (var i = children.Count - 1; i >= 0; i--)
        {
            var child = children[i];
            if (child.NestedClassName != null)
            {
                children.RemoveAt(i);

                nestedClasses.Add(new TemplateTree
                {
                    Root = child,
                    ClassName = child.NestedClassName,
                    NamespaceMap = namespaceMap,
                    FilePath = filePath,
                });
            }
            else
            {
                ExtractNestedClasses(child, nestedClasses, namespaceMap, filePath);
            }
        }
    }

    private static SourceSpan GetElementSpan(XElement element, string filePath)
    {
        var lineInfo = (IXmlLineInfo)element;
        if (!lineInfo.HasLineInfo())
        {
            return new SourceSpan(filePath, 0, 0);
        }

        var line = lineInfo.LineNumber;
        var col = lineInfo.LinePosition;
        var endCol = col + 1 + element.Name.LocalName.Length;
        return new SourceSpan(filePath, line, col, line, endCol);
    }

    private static SourceSpan GetAttributeSpan(XAttribute attr, string filePath)
    {
        var lineInfo = (IXmlLineInfo)attr;
        if (!lineInfo.HasLineInfo())
        {
            return new SourceSpan(filePath, 0, 0);
        }

        // Covers Name="Value" — the 3 accounts for =, opening ", and closing "
        var line = lineInfo.LineNumber;
        var col = lineInfo.LinePosition;
        var endCol = col + attr.Name.LocalName.Length + attr.Value.Length + 3;
        return new SourceSpan(filePath, line, col, line, endCol);
    }

    private static SourceSpan GetTextSpan(XText text, string filePath)
    {
        var lineInfo = (IXmlLineInfo)text;
        if (!lineInfo.HasLineInfo())
        {
            return new SourceSpan(filePath, 0, 0);
        }

        return new SourceSpan(filePath, lineInfo.LineNumber, lineInfo.LinePosition);
    }

    private static TemplateNode ParseElement(XElement element, string filePath)
    {
        var node = new TemplateNode
        {
            ElementName = element.Name.LocalName,
            ElementSpan = GetElementSpan(element, filePath),
        };

        // Process attributes
        foreach (var attr in element.Attributes())
        {
            // Skip namespace declarations
            if (attr.IsNamespaceDeclaration)
            {
                continue;
            }

            // Handle x: directives
            if (attr.Name.Namespace == _directivesNs)
            {
                if (attr.Name.LocalName == "Class")
                {
                    node.NestedClassName = attr.Value.Trim();
                    node.NestedClassNameSpan = GetAttributeSpan(attr, filePath);
                }
                else if (attr.Name.LocalName == "Arguments")
                {
                    var value = attr.Value.Trim();
                    if (value.StartsWith("{") && value.EndsWith("}"))
                    {
                        value = value.Substring(1, value.Length - 2).Trim();
                    }

                    node.ConstructorArguments = value;
                    node.ConstructorArgumentsSpan = GetAttributeSpan(attr, filePath);
                }
                else if (attr.Name.LocalName == "TypeArguments")
                {
                    var value = attr.Value.Trim();
                    if (value.StartsWith("{") && value.EndsWith("}"))
                    {
                        value = value.Substring(1, value.Length - 2).Trim();
                    }

                    node.TypeArguments = value;
                    node.TypeArgumentsSpan = GetAttributeSpan(attr, filePath);
                }
                else if (attr.Name.LocalName == "Name")
                {
                    node.Name = attr.Value;
                }

                continue;
            }

            // Handle bind: bindings
            if (attr.Name.Namespace == _bindingsNs)
            {
                var bindName = attr.Name.LocalName;
                node.Bindings[bindName] = attr.Value.Trim();
                node.BindingSpans[bindName] = GetAttributeSpan(attr, filePath);
                continue;
            }

            var name = attr.Name.LocalName;

            node.Attributes[name] = attr.Value;
            node.AttributeSpans[name] = GetAttributeSpan(attr, filePath);
        }

        // Process children
        foreach (var child in element.Elements())
        {
            var localName = child.Name.LocalName;

            // Check for property element syntax: ParentType.PropertyName
            if (localName.Contains("."))
            {
                var dotIndex = localName.IndexOf('.');
                var propertyName = localName.Substring(dotIndex + 1);

                // If the property element has no child elements but has text, treat as string value
                if (!child.HasElements)
                {
                    var propText = NormalizeTextContent(child);
                    if (propText != null)
                    {
                        node.Attributes[propertyName] = propText;
                        node.AttributeSpans[propertyName] = GetElementSpan(child, filePath);
                        continue;
                    }
                }

                var propertyChildren = new List<TemplateNode>();
                foreach (var propertyChild in child.Elements())
                {
                    propertyChildren.Add(ParseElement(propertyChild, filePath));
                }

                node.PropertyElements[propertyName] = propertyChildren;
            }
            else
            {
                node.Children.Add(ParseElement(child, filePath));
            }
        }

        // Capture text content from XText nodes
        node.TextContent = NormalizeTextContent(element);
        if (node.TextContent != null)
        {
            var firstText = element.Nodes().OfType<XText>().FirstOrDefault();
            if (firstText != null)
            {
                node.TextContentSpan = GetTextSpan(firstText, filePath);
            }
        }

        return node;
    }

    private static string? NormalizeTextContent(XElement element)
    {
        var textParts = new List<string>();
        foreach (var textNode in element.Nodes().OfType<XText>())
        {
            textParts.Add(textNode.Value);
        }

        if (textParts.Count == 0)
        {
            return null;
        }

        var raw = string.Join("", textParts);
        var lines = raw.Split('\n');

        // Trim trailing \r from each line (for CRLF)
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].EndsWith("\r"))
            {
                lines[i] = lines[i].Substring(0, lines[i].Length - 1);
            }
        }

        // Remove leading blank lines
        var start = 0;
        while (start < lines.Length && lines[start].Trim().Length == 0)
        {
            start++;
        }

        // Remove trailing blank lines
        var end = lines.Length - 1;
        while (end >= start && lines[end].Trim().Length == 0)
        {
            end--;
        }

        if (start > end)
        {
            return null;
        }

        // Compute minimum indentation of non-empty lines
        var minIndent = int.MaxValue;
        for (var i = start; i <= end; i++)
        {
            if (lines[i].Trim().Length == 0)
            {
                continue;
            }

            var indent = 0;
            while (indent < lines[i].Length && lines[i][indent] == ' ')
            {
                indent++;
            }

            if (indent < minIndent)
            {
                minIndent = indent;
            }
        }

        if (minIndent == int.MaxValue)
        {
            minIndent = 0;
        }

        // Strip common indent and join
        var result = new List<string>();
        for (var i = start; i <= end; i++)
        {
            if (lines[i].Trim().Length == 0)
            {
                result.Add("");
            }
            else
            {
                result.Add(lines[i].Substring(minIndent));
            }
        }

        var text = string.Join("\n", result);
        return text.Length > 0 ? text : null;
    }
}
