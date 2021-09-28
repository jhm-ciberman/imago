using System.Numerics;

namespace LifeSim.Rendering
{
    public struct Particle
    {
        public Vector3 Position;
        public float Size;
        public Vector3 Velocity;
        public float Life;
        public ColorF Color;

        public Particle(Vector3 position, Vector3 velocity, float life, float size, ColorF color)
        {
            this.Position = position;
            this.Velocity = velocity;
            this.Life = life;
            this.Size = size;
            this.Color = color;
        }
    }
}