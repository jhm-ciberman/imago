using System;

namespace Imago.Assets.Materials;

/// <summary>
/// Specifies the shader files for a surface material.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SurfaceShaderAttribute : Attribute
{
    /// <summary>
    /// Gets the path to the fragment shader file.
    /// </summary>
    public string? FragmentPath { get; }

    /// <summary>
    /// Gets the path to the vertex shader file.
    /// </summary>
    public string? VertexPath { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceShaderAttribute"/> class with a fragment shader only.
    /// </summary>
    /// <param name="fragmentPath">The path to the fragment shader file.</param>
    public SurfaceShaderAttribute(string fragmentPath)
    {
        this.FragmentPath = fragmentPath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceShaderAttribute"/> class with both fragment and vertex shaders.
    /// </summary>
    /// <param name="fragmentPath">The path to the fragment shader file.</param>
    /// <param name="vertexPath">The path to the vertex shader file.</param>
    public SurfaceShaderAttribute(string fragmentPath, string vertexPath)
    {
        this.FragmentPath = fragmentPath;
        this.VertexPath = vertexPath;
    }
}
