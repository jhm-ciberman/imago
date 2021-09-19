using System.Numerics;
using Veldrid.Utilities;

namespace LifeSim.Rendering
{
    public interface ICamera
    {
        Vector3 Position { get; }
        Matrix4x4 ViewProjectionMatrix { get; }
        BoundingFrustum FrustumForCulling { get; }
    }
}