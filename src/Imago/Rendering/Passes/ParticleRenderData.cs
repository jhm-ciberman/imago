using System.Numerics;
using System.Runtime.InteropServices;
using Support;

namespace Imago.Rendering.Passes;

[StructLayout(LayoutKind.Sequential)]
public struct ParticleRenderData
{
    public Vector4 PositionSize;
    public Color Color;

    public ParticleRenderData(Vector3 position, float size, Color color)
    {
        this.PositionSize = new Vector4(position, size);
        this.Color = color;
    }
}
