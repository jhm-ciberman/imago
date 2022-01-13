using System.Numerics;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering;

public interface ICamera
{
    Vector3 Position { get; }

    Vector3 Forward { get; }
    Vector3 Up { get; }
    Vector3 Right { get; }

    float NearPlane { get; }
    float FarPlane { get; }
    Matrix4x4 ViewMatrix { get; }
    Matrix4x4 ProjectionMatrix { get; }
    Matrix4x4 ViewProjectionMatrix { get; }
    BoundingFrustum FrustumForCulling { get; }
    ColorF? ClearColor { get; }
}