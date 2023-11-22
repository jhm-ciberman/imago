using System.Numerics;
using Support;

namespace Imago.Graphics.Particles;

public struct Particle
{
    public Vector3 Position;
    public float Size;
    public Vector3 Velocity;
    public float Life;
    public float MaxLife;
    public ColorF Color;
    public float DistanceToCamera;

    public Particle(Vector3 position, Vector3 velocity, float life, float size, ColorF color)
    {
        this.Position = position;
        this.Velocity = velocity;
        this.Life = life;
        this.MaxLife = life;
        this.Size = size;
        this.Color = color;
        this.DistanceToCamera = 0;
    }
}
