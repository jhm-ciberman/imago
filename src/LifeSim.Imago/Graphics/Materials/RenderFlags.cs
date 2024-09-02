using System;

namespace LifeSim.Imago.Graphics.Materials;

/// <summary>
/// <see cref="RenderFlags"/> is a bitmask that is used to specify the features that should
/// be used when rendering.
/// </summary>
[Flags]
internal enum RenderFlags : short
{
    None = 0,
    DoubleSided = 1 << 0,
    Wireframe = 1 << 1,
    Transparent = 1 << 2,
    AlphaTest = 1 << 3,
    DepthTest = 1 << 4,
    DepthWrite = 1 << 5,
    ReceiveShadows = 1 << 6,
    Fog = 1 << 7,
    PixelPerfactShadows = 1 << 8,
    ScisorTest = 1 << 9,
    ColorWrite = 1 << 10,
    ShadowCascades = 1 << 11,
    HalfLambert = 1 << 12,
}
