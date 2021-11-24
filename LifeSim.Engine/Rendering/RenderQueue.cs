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

        private readonly List<RenderIndex> _indices = new List<RenderIndex>(DEFAULT_CAPACITY);
        private readonly List<Renderable> _items = new List<Renderable>(DEFAULT_CAPACITY);

        public int Count => this._indices.Count;

        public Renderable this[int index] => this._items[this._indices[index].Index];

        public void Sort()
        {
            this._indices.Sort();
        }

        public void AddToRenderQueue(IReadOnlyList<Renderable> renderables, ref BoundingFrustum frustum, Vector3 cameraPosition)
        {
            this._indices.Clear();
            this._items.Clear();
            for (int i = 0; i < renderables.Count; i++)
            {
                Renderable renderable = renderables[i];
                if (renderable.Visible == false || renderable.Material == null || renderable.Mesh == null) continue;

                if (frustum.Contains(renderable.BoundingBox) != ContainmentType.Disjoint)
                {
                    ulong key = renderable.GetSortKey(cameraPosition);
                    this._indices.Add(new RenderIndex(key, this._items.Count));
                    this._items.Add(renderable);
                }
            }
        }

        public IEnumerator<Renderable> GetEnumerator()
        {
            return new Enumerator(this._indices, this._items);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this._indices, this._items);
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