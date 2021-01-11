namespace LifeSim.Engine
{
    public class Viewport
    {
        private uint _width;
        private uint _height;

        public event System.Action? onResize;

        public Viewport(uint width, uint height)
        {
            this._width = width;
            this._height = height;
        }

        public void Resize(uint width, uint height)
        {
            this._width = width;
            this._height = height;
            this.onResize?.Invoke();
        }

        public uint width => this._width;
        public uint height => this._height;
    }
}