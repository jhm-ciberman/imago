using System;
using Veldrid;

namespace LifeSim.Rendering
{
    public class Pass
    {
        public readonly Pipeline pipeline;
        public readonly ResourceSet? resourceSet;

        public Pass(Pipeline pipeline, ResourceSet? resourceSet)
        {
            this.pipeline = pipeline;
            this.resourceSet = resourceSet;
        }
    }
}