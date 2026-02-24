namespace Imago.Assets.Materials;

/// <summary>
/// Interface for materials that can be created via the material factory.
/// Uses C# 11 static abstract members for compile-time type safety.
/// </summary>
/// <typeparam name="TSelf">The concrete material type.</typeparam>
public interface ICreateableMaterial<TSelf> where TSelf : Material
{
    /// <summary>
    /// Creates a new instance of the material with the specified shaders.
    /// </summary>
    /// <param name="shaders">The compiled shaders for all render passes.</param>
    /// <returns>A new material instance.</returns>
    public static abstract TSelf Create(ShaderSet shaders);
}
