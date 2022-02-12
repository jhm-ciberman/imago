namespace LifeSim.Engine.Rendering;

public struct OffsetVertexData // It's 16 bytes only! 
{
    public uint TransformDataOffset { get; set; } // x
    public uint InstanceDataOffset { get; set; } // y
    public uint BoneDataOffset { get; set; } // z
    public uint PickingId { get; set; } // w
}