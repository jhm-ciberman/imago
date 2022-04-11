using System;

namespace LifeSim.Engine.Rendering;


[Flags]
public enum RenderFlags : byte
{
    None = 0,
    DoubleSided = 1 << 0,
    Wireframe = 1 << 1,
    Transparent = 1 << 2,
    AlphaTest = 1 << 3,
    DepthTest = 1 << 4,
    DepthWrite = 1 << 5,
    MousePick = 1 << 6,
}