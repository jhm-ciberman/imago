using System.Collections.Generic;
using Veldrid;
using Veldrid.SPIRV;

namespace LifeSim.Imago.Assets.Meshes;

/// <summary>
/// Defines the structure of a vertex by describing its attributes.
/// </summary>
public class VertexFormat
{
    /// <summary>
    /// Gets or sets the name of the vertex format.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the array of vertex layout descriptions that define the vertex structure.
    /// </summary>
    public VertexLayoutDescription[] Layouts { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this vertex format includes skinning information (bone indices and weights).
    /// </summary>
    public bool IsSkinned { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="VertexFormat"/> class.
    /// </summary>
    /// <param name="name">The name of the vertex format.</param>
    /// <param name="layouts">The array of vertex layout descriptions.</param>
    public VertexFormat(string name, params VertexLayoutDescription[] layouts)
    {
        this.Name = name;
        this.Layouts = layouts;
    }

    /// <summary>
    /// Gets the shader macro definitions required to support this vertex format.
    /// </summary>
    /// <returns>A list of macro definitions for the shader compiler.</returns>
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
