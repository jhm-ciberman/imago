using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class Pass
    {
        private static uint _count = 0;

        public readonly uint id;
        public readonly string shaderName;
        public readonly ResourceSet resourceSet;
        public readonly ResourceLayout resourceLayout;
        public readonly Description description;

        public struct Description
        {
            public BlendStateDescription blendState;
            public FaceCullMode faceCullMode;
            public OutputDescription outputDescription;
        }

        public Pass(string shaderName, ResourceSet resourceSet, ResourceLayout layout, Description description)
        {
            this.id = ++Pass._count;
            this.shaderName = shaderName;
            this.resourceSet = resourceSet;
            this.resourceLayout = layout;
            this.description = description;
        }
    }
}