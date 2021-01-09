using System.Collections.Generic;
using System.Collections;

namespace LifeSim 
{
    public class Grid<T> : IEnumerable<T>, IGridSource<T>
    {
        protected T[,] _data;

        public readonly Vector2Int size;

        public Grid(Vector2Int size) 
        {
            this.size = size;
            this._data = new T[size.x, size.y];
        }

        public IEnumerable<(Vector2Int, T)> cells
        {
            get
            {
                for (int x = 0; x < this.size.x; x++) 
                {
                    for (int y = 0; y < this.size.y; y++) 
                    {
                        yield return (new Vector2Int(x, y), this.Get(x, y));
                    }       
                }
            }
        }

        public T Get(int x, int y)
        {
            return this._data[x, y];
        }

        public T Get(Vector2Int coord)
        {
            return this.Get(coord.x, coord.y);
        }

        public T SafeGet(int x, int y, T defaultValue) 
        {
            return this.HasValue(x, y) ? this._data[x, y] : defaultValue;
        }

        public T SafeGet(Vector2Int coord, T defaultValue) 
        {
            return this.SafeGet(coord.x, coord.y, defaultValue);
        }

        public void Set(int x, int y, T value)
        {
            this._data[x, y] = value;
        }

        public void Set(Vector2Int coord, T value)
        {
            this.Set(coord.x, coord.y, value);
        }

        public bool HasValue(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < this.size.x && y < this.size.y);
        }

        public bool IsInside(Vector2Int coord)
        {
            return this.HasValue(coord.x, coord.y);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int x = 0; x < this.size.x; x++) 
            {
                for (int y = 0; y < this.size.y; y++) 
                {
                    yield return this._data[x, y];
                }       
            }
        }

        public void Fill(T value)
        {
            for (int x = 0; x < this.size.x; x++) 
            {
                for (int y = 0; y < this.size.y; y++) 
                {
                    this._data[x, y] = value;
                }       
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public T this[int x, int y]     { get => this.Get(x, y);  set => this.Set(x, y, value);  }
        public T this[Vector2Int coord] { get => this.Get(coord); set => this.Set(coord, value); }
    } 
}