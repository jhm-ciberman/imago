using System.Numerics;
using System.Runtime.InteropServices;

namespace LifeSim.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ParticleRenderData
    {
        public Vector3 Position;
        public float Size;
        public Color Color;

        public ParticleRenderData(Vector3 position, float size, Color color)
        {
            this.Position = position;
            this.Size = size;
            this.Color = color;
        }
    }
}