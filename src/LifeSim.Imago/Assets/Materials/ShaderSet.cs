namespace LifeSim.Imago.Assets.Materials;

/// <summary>
/// Contains the compiled shaders for all render passes of a surface material.
/// </summary>
public readonly struct ShaderSet
{
    /// <summary>
    /// Gets the shader for the forward rendering pass.
    /// </summary>
    public required Shader Forward { get; init; }

    /// <summary>
    /// Gets the shader for the shadow map rendering pass.
    /// </summary>
    public required Shader Shadow { get; init; }

    /// <summary>
    /// Gets the shader for the mouse picking pass.
    /// </summary>
    public required Shader Picking { get; init; }
}
