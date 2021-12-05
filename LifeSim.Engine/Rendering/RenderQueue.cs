using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public class RenderQueue : IEnumerable<Renderable>, IReadOnlyList<Renderable>, IReadOnlyCollection<Renderable>
    {
        private const int DEFAULT_CAPACITY = 250;

        private List<RenderIndex> _indicesCurrent = new List<RenderIndex>(DEFAULT_CAPACITY);
        private List<Renderable> _itemsCurrent = new List<Renderable>(DEFAULT_CAPACITY);

        private List<RenderIndex> _indicesSecondary = new List<RenderIndex>(DEFAULT_CAPACITY); // Used for double buffering (safe multi-threading)

        private List<Renderable> _itemsSecondary = new List<Renderable>(DEFAULT_CAPACITY);

        private readonly object _lock = new object();

        public int Count => this._indicesCurrent.Count;

        public Renderable this[int index] => this._itemsCurrent[this._indicesCurrent[index].Index];

        public void Sort()
        {
            lock (this._lock) // Thread safe, swap lists
            {
                (this._indicesCurrent, this._indicesSecondary) = (this._indicesSecondary, this._indicesCurrent);
                (this._itemsCurrent, this._itemsSecondary) = (this._itemsSecondary, this._itemsCurrent);
            }

            this._indicesCurrent.Sort();
        }



        public void AddToRenderQueue(IReadOnlyList<Renderable> renderables, ref BoundingFrustum frustum, Vector3 cameraPosition)
        {
            this._indicesCurrent.Clear();
            this._itemsCurrent.Clear();
            for (int i = 0; i < renderables.Count; i++)
            {
                Renderable renderable = renderables[i];
                if (renderable.Visible == false || renderable.Material == null || renderable.Mesh == null) continue;

                if (frustum.Contains(renderable.BoundingBox) != ContainmentType.Disjoint)
                {
                    ulong key = renderable.GetSortKey(cameraPosition);
                    this._indicesCurrent.Add(new RenderIndex(key, this._itemsCurrent.Count));
                    this._itemsCurrent.Add(renderable);
                }
            }
        }

        public IEnumerator<Renderable> GetEnumerator()
        {
            return new Enumerator(this._indicesCurrent, this._itemsCurrent);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this._indicesCurrent, this._itemsCurrent);
        }

        private struct Enumerator : IEnumerator<Renderable>
        {
            private readonly List<RenderIndex> _indices;
            private readonly List<Renderable> _items;
            private int _nextItemIndex;
            private Renderable? _current;

            public Enumerator(List<RenderIndex> indices, List<Renderable> items)
            {
                this._indices = indices;
                this._items = items;
                this._nextItemIndex = 0;
                this._current = default(Renderable);
            }

            public Renderable Current => this._current ?? throw new Exception();
            object? IEnumerator.Current => this.Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (this._nextItemIndex >= this._indices.Count)
                {
                    this._current = default(Renderable);
                    return false;
                }
                else
                {
                    var currentIndex = this._indices[this._nextItemIndex].Index;
                    this._current = this._items[currentIndex];
                    this._nextItemIndex += 1;
                    return true;
                }
            }

            public void Reset()
            {
                this._nextItemIndex = 0;
            }
        }

        public readonly struct RenderIndex : IComparable<RenderIndex>
        {
            public readonly ulong Key;
            public readonly int Index;

            public RenderIndex(ulong key, int index)
            {
                this.Key = key;
                this.Index = index;
            }

            public override string ToString()
            {
                return Convert.ToString((long)this.Key, 16).PadLeft(16, '0');
            }

            int IComparable<RenderIndex>.CompareTo(RenderIndex other)
            {
                return (this.Key < other.Key) ? -1 : (this.Key == other.Key) ? 0 : 1;
            }
        }
    }
}