namespace LifeSim.Rendering
{
    public struct OffsetVertexData // It's 16 bytes only! 
    {
        public uint transformDataOffset; // x
        public uint instanceDataOffset; // y
        public uint boneDataOffset; // z
        public uint pickingId; // w

        public OffsetVertexData(uint transformDataOffset, uint instanceDataOffset, uint boneDataOffset, uint pickingId)
        {
            this.transformDataOffset = transformDataOffset;
            this.instanceDataOffset = instanceDataOffset;
            this.boneDataOffset = boneDataOffset;
            this.pickingId = pickingId;
        }
    }
}