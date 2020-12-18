using System.Numerics;

namespace LifeSim.Rendering
{
    public readonly struct Vertex
    {
        public readonly Vector3 pos;
        public readonly Vector2 uv;
        public readonly Vector2 light;

        public Vertex(Vector3 pos, Vector2 uv, Vector2 light)
        {
            this.pos = pos;
            this.uv = uv;
            this.light = light;
        }

        public Vertex(Vector3 pos, Vector2 uv)
        {
            this.pos = pos;
            this.uv = uv;
            this.light = Vector2.One;
        }

        public Vertex(float x, float y, float z, float u, float v) 
            : this(new Vector3(x, y, z), new Vector2(u, v)) {}

        public Vertex(float x, float y, float z, float u, float v, Vector2 light) 
            : this(new Vector3(x, y, z), new Vector2(u, v), light) {}

        public float x => this.pos.X;
        public float y => this.pos.Y;
        public float z => this.pos.Z;
        public float u => this.uv.X;
        public float v => this.uv.Y;

        public Vertex WithSunlight() => new Vertex(this.pos, this.uv, Vector2.One);
    }
}