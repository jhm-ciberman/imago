namespace LifeSim.Rendering
{
    public struct OffsetVertexData // It's 16 bytes only! 
    {
        public uint TransformDataOffset { get; set; } // x
        public uint InstanceDataOffset { get; set; } // y
        public uint BoneDataOffset { get; set; } // z
        public uint PickingId { get; set; } // w

        public OffsetVertexData(uint transformDataOffset, uint instanceDataOffset, uint boneDataOffset, uint pickingId)
        {
            this.TransformDataOffset = transformDataOffset;
            this.InstanceDataOffset = instanceDataOffset;
            this.BoneDataOffset = boneDataOffset;
            this.PickingId = pickingId;
        }
    }
}