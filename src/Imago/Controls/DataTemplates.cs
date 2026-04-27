using System;
using System.Collections;
using System.Collections.Generic;

namespace Imago.Controls;

/// <summary>
/// A composite data template that dispatches to the first matching child template.
/// </summary>
/// <remarks>
/// Children are inspected in insertion order; the first one whose
/// <see cref="IDataTemplate.Match"/> returns <see langword="true"/> is used to build the control.
/// If no child matches, <see cref="CreateItem"/> throws.
/// </remarks>
public class DataTemplates : IDataTemplate, IEnumerable<IDataTemplate>
{
    private readonly List<IDataTemplate> _children = new List<IDataTemplate>();

    /// <summary>
    /// Gets the child templates in dispatch order.
    /// </summary>
    public IReadOnlyList<IDataTemplate> Children => this._children;

    /// <summary>
    /// Adds a child template to the dispatch list.
    /// </summary>
    /// <param name="template">The template to add.</param>
    public void Add(IDataTemplate template)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        this._children.Add(template);
    }

    /// <inheritdoc/>
    public bool Match(object item)
    {
        foreach (var child in this._children)
        {
            if (child.Match(item))
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public Control CreateItem(object item)
    {
        foreach (var child in this._children)
        {
            if (child.Match(item))
            {
                return child.CreateItem(item);
            }
        }

        throw new InvalidOperationException(
            $"No data template in this {nameof(DataTemplates)} matches an item of type '{item.GetType().FullName}'.");
    }

    /// <inheritdoc/>
    public IEnumerator<IDataTemplate> GetEnumerator() => this._children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this._children.GetEnumerator();
}
