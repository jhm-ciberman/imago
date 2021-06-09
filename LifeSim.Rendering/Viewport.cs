namespace LifeSim.Rendering
{
    public class Viewport
    {
        public event System.Action<Viewport>? onResized;

        private uint _x;
        private uint _y;
        private uint _width;
        private uint _height;

        public Viewport(uint width, uint height) : this(0, 0, width, height) { }

        public Viewport(uint x, uint y, uint width, uint height)
        {
            this._x = x;
            this._y = y;
            this._width = width;
            this._height = height;
        }

        public void Resize(uint width, uint height)
        {
            this._width = width;
            this._height = height;
            this.onResized?.Invoke(this);
        }

        public void Move(uint x, uint y)
        {
            this._x = x;
            this._y = y;
        }

        public uint width => this._width;
        public uint height => this._height;

        public uint x => this._x;
        public uint y => this._y;
    }
}