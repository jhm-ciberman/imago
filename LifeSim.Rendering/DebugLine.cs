using System.Numerics;

namespace LifeSim.Rendering
{
    public struct DebugLine
    {
        public Vector3 Start;
        public Vector3 End;
        public Color Color;
        public float LifeTime;
        public bool DrawInFront;
    }
}