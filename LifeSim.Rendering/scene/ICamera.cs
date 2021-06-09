using System.Numerics;

namespace LifeSim.Rendering
{
    public interface ICamera
    {
        Vector3 position { get; }
        Matrix4x4 viewProjectionMatrix { get; }
        ICamera frustumCullingCamera { get; }
    }
}