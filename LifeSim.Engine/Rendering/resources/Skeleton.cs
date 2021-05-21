using System.Numerics;
using System.Runtime.CompilerServices;

namespace LifeSim.Engine.Rendering
{
    public class Skeleton
    {
        public const int MAX_NUMBER_OF_BONES = 64;

        public unsafe struct BonesData
        {
            public fixed float boneData[Skeleton.MAX_NUMBER_OF_BONES * 16];
        }

        public Matrix4x4[] bonesMatrices = new Matrix4x4[Skeleton.MAX_NUMBER_OF_BONES];

        /*
        public unsafe BonesData StoreBoneData(Skeleton skeleton)
        {
            BonesData b;
            fixed (Matrix4x4* ptr = skeleton.bonesMatrices) {
                Unsafe.CopyBlock(&b, ptr, Skeleton.MAX_NUMBER_OF_BONES * 4 * 16);
            }
            return b;
        }
        */
    }
}