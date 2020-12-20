using System.Numerics;
using System.Runtime.CompilerServices;

namespace LifeSim.Rendering
{
    public unsafe struct BonesInfo
    {
        public const int maxNumberOfBones = 10;

        public Matrix4x4[] BonesTransformations;

        public Blittable GetBlittable()
        {
            Blittable b;
            fixed (Matrix4x4* ptr = BonesTransformations)
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
            return new BonesInfo() { BonesTransformations = new Matrix4x4[BonesInfo.maxNumberOfBones] };
        }
    }
}