using System;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class Pass
    {
        private static uint _count = 0;

        public readonly string shaderName;
        public readonly ResourceSet? resourceSet;
        public readonly ResourceLayout? resourceLayout;
        public readonly Description description;
        public readonly uint id;

        public struct Description
        {
            public BlendStateDescription blendState;
            public FaceCullMode faceCullMode;
            public OutputDescription outputDescription;
        }

        public Pass(GraphicsDevice gd, string shaderName, ResourceSetDescription? resource, Description description)
        {
            this.shaderName = shaderName;
            this.resourceSet = resource.HasValue ? gd.ResourceFactory.CreateResourceSet(resource.Value) : null;
            this.resourceLayout = resource.HasValue ? resource.Value.Layout : null;
            this.description = description;
            this.id = ++Pass._count;
        }
    }
}