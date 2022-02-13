using System;

namespace LifeSim.Engine.Rendering;

[Flags]
public enum RenderQueueFlags : byte
{
    None = 0,
    Opaque = 1 << 0,
    Transparent = 1 << 1,
    ShadowCaster = 1 << 2,
    All = Opaque | Transparent | ShadowCaster,
}