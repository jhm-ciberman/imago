using System;

namespace LifeSim.Imago.Assets.Materials;

/// <summary>
/// Marks a texture property on a material class. Used by the material factory
/// to determine the resource layout for the shader.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class MaterialTextureAttribute : Attribute
{
    /// <summary>
    /// Gets the base name for the texture uniform in the shader.
    /// The shader will have uniforms named {Name}Texture and {Name}Sampler.
    /// If null, derived from the property name.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialTextureAttribute"/> class.
    /// The base name is derived from the property name.
    /// </summary>
    public MaterialTextureAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialTextureAttribute"/> class
    /// with an explicit base name.
    /// </summary>
    /// <param name="name">The base name for the shader uniforms ({name}Texture, {name}Sampler).</param>
    public MaterialTextureAttribute(string name)
    {
        this.Name = name;
    }
}
