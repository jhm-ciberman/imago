namespace LifeSim.Engine.Rendering;

/// <summary>
/// Determines the mode the object will use to cast shadows.
/// </summary>
public enum ShadowCasting
{
    /// <summary>
    /// The object will cast shadows. This is the default mode.
    /// </summary>
    CastShadows,

    /// <summary>
    /// The object will not cast shadows.
    /// </summary>
    NoShadows,

    /// <summary>
    /// The object will only cast shadows and the object itself will not be visible.
    /// </summary>
    OnlyShadows,
}