using System.Numerics;
using System.Runtime.CompilerServices;

namespace LifeSim.Engine.Rendering
{
    public unsafe struct BonesInfo
    {
        public const int MAX_NUMBER_OF_BONES = 64;
        
        public Matrix4x4[] bonesMatrices;

        public Blittable GetBlittable()
        {
            Blittable b;
            fixed (Matrix4x4* ptr = this.bonesMatrices)
            {
                Unsafe.CopyBlock(&b, ptr, BonesInfo.MAX_NUMBER_OF_BONES * 4 * 16);
            }

            return b;
        }

        public struct Blittable
        {
            public fixed float boneData[BonesInfo.MAX_NUMBER_OF_BONES * 16];
        }

        internal static BonesInfo New()
        {
            return new BonesInfo() { bonesMatrices = new Matrix4x4[BonesInfo.MAX_NUMBER_OF_BONES] };
        }
    }
}