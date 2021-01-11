using System.Numerics;
using System.Runtime.CompilerServices;

namespace LifeSim.Engine.Rendering
{
    public unsafe struct BonesInfo
    {
        public const int maxNumberOfBones = 64;

        public Matrix4x4[] bonesMatrices;

        public Blittable GetBlittable()
        {
            Blittable b;
            fixed (Matrix4x4* ptr = bonesMatrices)
            {
                Unsafe.CopyBlock(&b, ptr, BonesInfo.maxNumberOfBones * 4 * 16);
            }

            return b;
        }

        public struct Blittable
        {
            public fixed float BoneData[BonesInfo.maxNumberOfBones * 16];
        }

        internal static BonesInfo New()
        {
            return new BonesInfo() { bonesMatrices = new Matrix4x4[BonesInfo.maxNumberOfBones] };
        }
    }
}