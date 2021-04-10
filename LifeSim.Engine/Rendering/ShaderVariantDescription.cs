using System;
using System.Diagnostics.CodeAnalysis;

namespace LifeSim.Engine.Rendering
{
    public struct ShaderVariantDescription : IEquatable<ShaderVariantDescription>
    {
        public string shaderName;
        public string[] keywords;

        public ShaderVariantDescription(string shaderName, string[] keywords)
        {
            this.shaderName = shaderName;
            this.keywords = keywords;
        }

        public bool Equals([AllowNull] ShaderVariantDescription other)
        {
            return (this.shaderName == other.shaderName && Array.Equals(this.keywords, other.keywords));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.shaderName, this.keywords);
        }
    }
}