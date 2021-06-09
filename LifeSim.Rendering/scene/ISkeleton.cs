using System.Numerics;

namespace LifeSim.Rendering
{
    public interface ISkeleton 
    {
        Matrix4x4[] bonesMatrices { get;}
        
        void UpdateMatrices(ref Matrix4x4 inverseMeshWorldMatrix);
    }
}