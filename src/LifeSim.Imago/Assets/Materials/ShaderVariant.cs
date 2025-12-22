using System;
using LifeSim.Imago.Assets.Meshes;
using Veldrid;

namespace LifeSim.Imago.Assets.Materials;

/// <summary>
/// Represents a specific variant of a shader, tailored for a particular vertex format and set of render flags.
/// </summary>
internal class ShaderVariant : IDisposable
{
    /// <summary>
    /// Gets the parent <see cref="Shader"/> that this variant belongs to.
    /// </summary>
    public Shader Shader { get; }

    /// <summary>
    /// Gets the <see cref="VertexFormat"/> that this shader variant is compatible with.
    /// </summary>
    public VertexFormat VertexFormat { get; }

    /// <summary>
    /// Gets the compiled Veldrid <see cref="Veldrid.Shader"/> objects for this variant.
    /// </summary>
    public Veldrid.Shader[] VeldridShaders { get; }

    /// <summary>
    /// Gets the <see cref="ShaderSetDescription"/> for this shader variant.
    /// </summary>
    public ShaderSetDescription ShaderSetDescription { get; internal set; }

    /// <summary>
    /// Gets the resource layout for the material, inherited from the parent <see cref="Shader"/>.
    /// </summary>
    public ResourceLayout MaterialResourceLayout => this.Shader.MaterialResourceLayout;

    /// <summary>
    /// Gets the <see cref="RenderFlags"/> associated with this shader variant.
    /// </summary>
    public RenderFlags Flags { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShaderVariant"/> class.
    /// </summary>
    /// <param name="shader">The parent <see cref="Shader"/>.</param>
    /// <param name="vertexFormat">The vertex format for this variant.</param>
    /// <param name="shaders">The compiled Veldrid <see cref="Veldrid.Shader"/> objects.</param>
    /// <param name="flags">The <see cref="RenderFlags"/> for this variant.</param>
    internal ShaderVariant(Shader shader, VertexFormat vertexFormat, Veldrid.Shader[] shaders, RenderFlags flags)
    {
        this.Shader = shader;
        this.VertexFormat = vertexFormat;
        this.VeldridShaders = shaders;
        this.Flags = flags;
        this.ShaderSetDescription = new ShaderSetDescription(vertexFormat.Layouts, shaders);
    }

    /// <summary>
    /// Disposes the shader variant and its associated Veldrid shader resources.
    /// </summary>
    public void Dispose()
    {
        foreach (var shader in this.VeldridShaders)
        {
            shader.Dispose();
        }
    }
}
