using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public readonly struct RenderItem
    {
        public readonly Shader shader;
        public readonly Veldrid.ResourceSet transformResourceSet;
        public readonly Veldrid.ResourceSet instanceResourceSet;
        public readonly Veldrid.ResourceSet materialResourceSet;
        public readonly Mesh mesh;
        public readonly uint transformBufferOffset;
        public readonly uint instanceBufferOffset;

        public readonly uint pickingID;

        public RenderItem(Mesh mesh, Shader shader, uint transformBufferOffset, uint pickingID, ResourceSet transformResourceSet, ResourceSet materialResourceSet, ResourceSet instanceResourceSet, uint instanceBufferOffset)
        {
            this.mesh = mesh;
            this.shader = shader;
            this.pickingID = pickingID;
            this.transformBufferOffset = transformBufferOffset;
            this.transformResourceSet = transformResourceSet;
            this.materialResourceSet = materialResourceSet;
            this.instanceResourceSet = instanceResourceSet;
            this.instanceBufferOffset = instanceBufferOffset;
        }
    }
}