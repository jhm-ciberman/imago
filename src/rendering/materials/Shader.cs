using System.Collections.Generic;
using Veldrid;

namespace LifeSim.Rendering
{
    public class Shader
    {
        public readonly ShaderSetDescription shaderSet; 
        public readonly ResourceLayout? passResourcelayout;
        public readonly ResourceLayout? materialResourcelayout;
        public readonly ResourceLayout? objectResourcelayout;

        public Shader(
            ShaderSetDescription shaderSet, 
            ResourceLayout? passResourcelayout,
            ResourceLayout? materialResourcelayout,
            ResourceLayout? objectResourcelayout
        )
        {
            this.shaderSet = shaderSet;
            this.passResourcelayout = passResourcelayout;
            this.materialResourcelayout = materialResourcelayout;
            this.objectResourcelayout = objectResourcelayout;
        }

        public ResourceLayout[] GetResourceLayouts()
        {
            List<ResourceLayout> list = new List<ResourceLayout>(3);

            if (this.passResourcelayout != null)     list.Add(this.passResourcelayout);
            if (this.materialResourcelayout != null) list.Add(this.materialResourcelayout);
            if (this.objectResourcelayout != null)   list.Add(this.objectResourcelayout);

            return list.ToArray();
        }
    }
}