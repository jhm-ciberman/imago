using System;

namespace Imago.Assets.Materials;

/// <summary>
/// <see cref="RenderFlags"/> is a bitmask that is used to specify the features that should
/// be used when rendering.
/// </summary>
[Flags]
internal enum RenderFlags : short
{
    None = 0,
    /// <summary>
    /// Specifies that the material should be rendered as double-sided, disabling back-face culling.
    /// </summary>
    DoubleSided = 1 << 0,
    /// <summary>
    /// Specifies that the material should be rendered in wireframe mode.
    /// </summary>
    Wireframe = 1 << 1,
    /// <summary>
    /// Specifies that the material supports transparency.
    /// </summary>
    Transparent = 1 << 2,
    /// <summary>
    /// Specifies that alpha testing should be enabled for the material.
    /// </summary>
    AlphaTest = 1 << 3,
    /// <summary>
    /// Specifies that depth testing should be enabled for the material.
    /// </summary>
    DepthTest = 1 << 4,
    /// <summary>
    /// Specifies that writing to the depth buffer should be enabled for the material.
    /// </summary>
    DepthWrite = 1 << 5,
    /// <summary>
    /// Specifies that the material can receive shadows.
    /// </summary>
    ReceiveShadows = 1 << 6,
    /// <summary>
    /// Specifies that fog should be applied to the material.
    /// </summary>
    Fog = 1 << 7,
    /// <summary>
    /// Specifies that pixel-perfect shadows should be used for the material.
    /// </summary>
    PixelPerfectShadows = 1 << 8,
    /// <summary>
    /// Specifies that scissor testing should be enabled for the material.
    /// </summary>
    ScisorTest = 1 << 9,
    /// <summary>
    /// Specifies that writing to the color buffer should be enabled for the material.
    /// </summary>
    ColorWrite = 1 << 10,
    /// <summary>
    /// Specifies that shadow cascades should be used for the material.
    /// </summary>
    ShadowCascades = 1 << 11,
    /// <summary>
    /// Specifies that half-Lambert lighting should be used for the material.
    /// </summary>
    HalfLambert = 1 << 12,
}
