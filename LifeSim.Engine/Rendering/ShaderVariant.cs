using System;
using System.Diagnostics.CodeAnalysis;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class ShaderVariant : IEquatable<ShaderVariant>
    {
        public ShaderVariantDescription description;

        public Veldrid.Shader[] shaders;

        //public ResourceLayout[] resourceLayouts;

        public ShaderVariant(ShaderVariantDescription description, Veldrid.Shader[] shaders) //, ResourceLayout[] resourceLayouts)
        {
            this.description = description;
            this.shaders = shaders;
            //this.resourceLayouts = resourceLayouts;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ShaderVariant other) {
                return this.Equals(other);
            }
            return false;
        }

        public bool Equals(ShaderVariant? other)
        {
            if (other == null) return false;
            return this.description.Equals(other.description);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}