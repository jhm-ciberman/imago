using System.Numerics;

namespace LifeSim.Rendering
{
    public interface ISkeleton
    {
        Matrix4x4[] BonesMatrices { get; }

        void UpdateMatrices(ref Matrix4x4 inverseMeshWorldMatrix);
    }
}