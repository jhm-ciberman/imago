using System;
using System.Diagnostics.CodeAnalysis;

namespace LifeSim.Rendering
{
    public struct ShaderVariant : IEquatable<ShaderVariant>
    {
        public string shaderName;
        public string[] keywords;

        public ShaderVariant(string shaderName, string[] keywords)
        {
            this.shaderName = shaderName;
            this.keywords = keywords;
        }

        public bool Equals([AllowNull] ShaderVariant other)
        {
            return (this.shaderName == other.shaderName && Array.Equals(this.keywords, other.keywords));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.shaderName, this.keywords);
        }
    }
}