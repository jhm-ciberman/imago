using System.Collections.Generic;

namespace Imago.Generators;

/// <summary>
/// Collects .NET namespace imports and type aliases from <c>&lt;?using ...?&gt;</c> processing instructions.
/// </summary>
internal sealed class XmlNamespaceMap
{
    private readonly List<string> _namespaces = [];
    private readonly Dictionary<string, string> _aliases = [];

    /// <summary>
    /// Registers a namespace import.
    /// </summary>
    /// <param name="clrNamespace">The .NET namespace.</param>
    public void AddUsing(string clrNamespace)
    {
        if (!this._namespaces.Contains(clrNamespace))
        {
            this._namespaces.Add(clrNamespace);
        }
    }

    /// <summary>
    /// Registers a type alias that maps an element name directly to a fully qualified type name.
    /// </summary>
    /// <param name="alias">The alias name used as the element name in the template.</param>
    /// <param name="fullyQualifiedName">The fully qualified .NET type name.</param>
    public void AddAlias(string alias, string fullyQualifiedName)
    {
        this._aliases[alias] = fullyQualifiedName;
    }

    /// <summary>
    /// Attempts to resolve an element name as a type alias.
    /// </summary>
    /// <param name="elementName">The element name to look up.</param>
    /// <param name="fullyQualifiedName">The resolved fully qualified type name, if found.</param>
    /// <returns><c>true</c> if the element name matched a registered alias.</returns>
    public bool TryResolveAlias(string elementName, out string fullyQualifiedName)
    {
        return this._aliases.TryGetValue(elementName, out fullyQualifiedName);
    }

    /// <summary>
    /// Gets all candidate fully qualified names for an element,
    /// searching all imported namespaces.
    /// </summary>
    /// <param name="elementName">The local element name.</param>
    /// <returns>A list of candidate fully qualified names to try against the compilation.</returns>
    public List<string> GetCandidateNames(string elementName)
    {
        var candidates = new List<string>(this._namespaces.Count);
        foreach (var ns in this._namespaces)
        {
            candidates.Add(ns + "." + elementName);
        }

        return candidates;
    }

    /// <summary>
    /// Gets all .NET namespaces registered in this map, including namespaces extracted from aliases.
    /// </summary>
    public IEnumerable<string> GetAllNamespaces()
    {
        var all = new HashSet<string>(this._namespaces);
        foreach (var fqn in this._aliases.Values)
        {
            var lastDot = fqn.LastIndexOf('.');
            if (lastDot > 0)
            {
                all.Add(fqn.Substring(0, lastDot));
            }
        }

        return all;
    }
}
