namespace LifeSim.Rendering
{
    public class Viewport
    {
        public event System.Action<Viewport>? onResized;

        public Viewport(uint width, uint height) : this(0, 0, width, height) { }

        public Viewport(uint x, uint y, uint width, uint height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public void Resize(uint width, uint height)
        {
            this.Width = width;
            this.Height = height;
            this.onResized?.Invoke(this);
        }

        public void Move(uint x, uint y)
        {
            this.X = x;
            this.Y = y;
        }

        public uint Width { get; private set; }
        public uint Height { get; private set; }
        public uint X { get; private set; }
        public uint Y { get; private set; }
    }
}