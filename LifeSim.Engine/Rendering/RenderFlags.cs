namespace LifeSim.Engine.Rendering;


public enum RenderFlags : byte
{
    None = 0,
    DoubleSided = 1 << 0,
    Wireframe = 1 << 1,
    AlphaBlending = 1 << 2,
    DepthTest = 1 << 3,
    DepthWrite = 1 << 4,
    Default = DepthTest | DepthWrite,
}