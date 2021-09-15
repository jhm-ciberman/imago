using System.Numerics;
using Veldrid.Utilities;

namespace LifeSim.Rendering
{
    public interface ICamera
    {
        Vector3 position { get; }
        Matrix4x4 viewProjectionMatrix { get; }
        BoundingFrustum frustumForCulling { get; }
    }
}