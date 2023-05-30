using System;

namespace Imago.Rendering;

/// <summary>
/// <see cref="RenderFlags"/> is a bitmask that is used to specify the features that should
/// be used when rendering.
/// </summary>
[Flags]
public enum RenderFlags : short
{
    None = 0,
    DoubleSided = 1 << 0,
    Wireframe = 1 << 1,
    Transparent = 1 << 2,
    AlphaTest = 1 << 3,
    DepthTest = 1 << 4,
    DepthWrite = 1 << 5,
    MousePick = 1 << 6,
    ReceiveShadows = 1 << 7,
    Fog = 1 << 8,
    PixelPerfactShadows = 1 << 9,
    ScisorTest = 1 << 10,
}
