using Texture = LifeSim.Imago.Assets.Textures.Texture;

namespace LifeSim.Imago.Assets.Materials;

/// <summary>
/// The default surface material using standard lighting.
/// </summary>
[SurfaceShader(fragmentPath: "surfaces/standard.frag.glsl", vertexPath: "surfaces/standard.vert.glsl")]
public sealed class StandardMaterial : Material, ICreateableMaterial<StandardMaterial>
{
    private Texture? _texture;

    /// <summary>
    /// Creates a new instance of the <see cref="StandardMaterial"/> class.
    /// </summary>
    /// <param name="shaders">The compiled shaders for all render passes.</param>
    /// <returns>A new <see cref="StandardMaterial"/> instance.</returns>
    public static StandardMaterial Create(ShaderSet shaders) => new(shaders);

    private StandardMaterial(ShaderSet shaders) : base(shaders)
    {
    }

    /// <summary>
    /// Gets or sets the texture.
    /// </summary>
    [MaterialTexture("Surface")]
    public Texture? Texture
    {
        get => this._texture;
        set => this.SetTexture(ref this._texture, index: 0, value);
    }
}
