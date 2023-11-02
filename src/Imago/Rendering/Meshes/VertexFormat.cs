using System.Collections.Generic;
using Veldrid;
using Veldrid.SPIRV;

namespace Imago.Rendering.Meshes;

/// <summary>
/// Represents a vertex format.
/// </summary>
public class VertexFormat
{
    /// <summary>
    /// Gets or sets the name of the vertex format.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the vertex layouts.
    /// </summary>
    public VertexLayoutDescription[] Layouts { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the vertex format is skinned.
    /// </summary>
    public bool IsSkinned { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="VertexFormat"/> class.
    /// </summary>
    /// <param name="name">The name of the vertex format.</param>
    /// <param name="layouts">The vertex layouts.</param>
    public VertexFormat(string name, params VertexLayoutDescription[] layouts)
    {
        this.Name = name;
        this.Layouts = layouts;
    }

    /// <summary>
    /// Gets the vertex shader macros for the vertex format.
    /// </summary>
    public List<MacroDefinition> GetMacroDefinitions()
    {
        var macros = new List<MacroDefinition>();
        foreach (var layout in this.Layouts)
        {
            foreach (var element in layout.Elements)
            {
                macros.Add(new MacroDefinition("USE_" + element.Name.ToUpperInvariant()));
            }
        }
        return macros;
    }
}
