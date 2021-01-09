namespace LifeSim
{
    public struct Kernel<T> : ISafeKernel<T>
    {
        private IGridSource<T> _source;

        private Vector2Int _center;

        internal Kernel(IGridSource<T> source, Vector2Int center)
        {
            this._source = source;
            this._center = center;
        }

        internal Kernel(IGridSource<T> source, int centerX, int centerY) 
            : this(source, new Vector2Int(centerX, centerY))
        {
            //
        }

        public T value => this._source[this._center.x, this._center.y];

        public T Get(int x, int y)  => this._source[this._center.x + x, this._center.y + y];

        public T SafeGet(int x, int y, T defaultValue) => this._source.HasValue(this._center.x + x, this._center.y + y) ? this.Get(x, y) : defaultValue;

        public T SafeGet(Vector2Int v, T defaultValue) => this.SafeGet(v.x, v.y, defaultValue);
        public T this[int x, int y] => this.Get(x, y);
        public T Get(Vector2Int v) => this.Get(v.x, v.y);
    }
}