using System.Numerics;

namespace LifeSim.Engine.Rendering
{
    public struct ShadowCascadeInfo
    {
        public Matrix4x4 ShadowMapMatrix;
        public float SplitDistance;
        public Vector3 CameraPosition;
    }
}